using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GLSLShaderLab.Core.Models;
using GLSLShaderLab.Core.Services;
using GLSLShaderLab.Engine.Services;
using OpenTK.GLControl;

namespace GLSLShaderLab.App.Wpf;

public partial class MainWindow : Window
{
    private readonly SessionStore _sessionStore = new();
    private readonly ShaderToyRenderer _renderer = new();
    private readonly DispatcherTimer _renderTimer;
    private readonly DispatcherTimer _compileDebounceTimer;
    private readonly Stopwatch _frameStopwatch = Stopwatch.StartNew();

    private GLControl? _glControl;
    private ShaderDocument _document = ShaderTemplateCatalog.CreateDefaultDocument();
    private string? _currentFilePath;
    private DateTime _fpsWindowStart = DateTime.UtcNow;
    private int _framesInWindow;
    private bool _isFullscreen;
    private WindowState _previousWindowState;
    private WindowStyle _previousWindowStyle;
    private ResizeMode _previousResizeMode;
    private bool _previousTopmost;
    private GridLength _previousEditorColumnWidth;
    private GridLength _previousPreviewColumnWidth;
    private GridLength _previousDiagnosticsRowHeight;
    private GridLength _previousPreviewHeaderRowHeight;
    private GridLength _previousPreviewChannelsRowHeight;
    private Thickness _previousPreviewBorderMargin;
    private IReadOnlyList<ModelAsset> _availableModels = Array.Empty<ModelAsset>();
    private bool _isOrbitingCamera;
    private System.Drawing.Point _lastMousePosition;

    public MainWindow()
    {
        InitializeComponent();

        _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _renderTimer.Tick += RenderTimer_Tick;

        _compileDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(450)
        };
        _compileDebounceTimer.Tick += CompileDebounceTimer_Tick;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _document = _sessionStore.LoadOrDefault();
        if (string.IsNullOrWhiteSpace(_document.VertexSource))
        {
            _document.VertexSource = ShaderTemplateCatalog.ModelVertex;
        }

        AutoCompileCheckBox.IsChecked = _document.AutoCompile;
        EditorTextBox.Text = _document.FragmentSource;
        VertexEditorTextBox.Text = _document.VertexSource;

        foreach (var template in ShaderTemplateCatalog.Templates)
        {
            TemplateComboBox.Items.Add(template.Name);
        }

        PopulateModelsCombo();
        RenderModeComboBox.SelectedIndex = _document.RenderMode == RenderMode.ThreeD ? 1 : 0;

        InitializeGlHost();
        if (_glControl is not null)
        {
            _glControl.MakeCurrent();
            AppendCompileResult(_renderer.SetRenderMode(_document.RenderMode, _document.VertexSource));
        }

        Update3DControlsState();
        _renderer.SetPaused(_document.IsPaused);
        PlayPauseButton.Content = _document.IsPaused ? "Play" : "Pause";

        if (_document.IsFullscreen)
        {
            ToggleFullscreen();
        }

        _renderTimer.Start();
        AppendDiagnostic("Studio ready.");
    }

    private void InitializeGlHost()
    {
        var glSettings = new GLControlSettings
        {
            API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
            APIVersion = new Version(3, 3),
            Profile = OpenTK.Windowing.Common.ContextProfile.Core,
            IsEventDriven = false
        };

        _glControl = new GLControl(glSettings)
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = System.Drawing.Color.Black
        };

        _glControl.Resize += (_, _) =>
        {
            if (_glControl.ClientSize.Width <= 0 || _glControl.ClientSize.Height <= 0) return;
            _glControl.MakeCurrent();
            _renderer.Resize(_glControl.ClientSize.Width, _glControl.ClientSize.Height);
        };

        _glControl.MouseMove += (_, args) =>
        {
            _renderer.SetMouse(args.X, args.Y, args.Button == System.Windows.Forms.MouseButtons.Left);

            if (_document.RenderMode == RenderMode.ThreeD && _isOrbitingCamera)
            {
                var dx = args.X - _lastMousePosition.X;
                var dy = args.Y - _lastMousePosition.Y;
                _renderer.RotateCamera(dx * 0.2f, -dy * 0.2f);
                _lastMousePosition = new System.Drawing.Point(args.X, args.Y);
            }
        };

        _glControl.MouseDown += (_, args) =>
        {
            _renderer.SetMouse(args.X, args.Y, true);
            if (_document.RenderMode == RenderMode.ThreeD && args.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _isOrbitingCamera = true;
                _lastMousePosition = new System.Drawing.Point(args.X, args.Y);
            }
            _glControl.Focus();
        };

        _glControl.MouseUp += (_, args) =>
        {
            _renderer.SetMouse(args.X, args.Y, false);
            if (args.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _isOrbitingCamera = false;
            }
        };

        _glControl.MouseWheel += (_, args) =>
        {
            if (_document.RenderMode == RenderMode.ThreeD)
            {
                _renderer.ZoomCamera(args.Delta / 120f * 2f);
            }
        };

        _glControl.KeyDown += (_, args) =>
        {
            if (args.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                ExitFullscreenIfActive();
            }
        };

        PreviewHost.Child = _glControl;
        _glControl.MakeCurrent();

        _renderer.Initialize(
            Math.Max(1, _glControl.ClientSize.Width),
            Math.Max(1, _glControl.ClientSize.Height),
            _document.FragmentSource);

        TryLoadInitialModel();

        foreach (var channel in _document.Channels.Where(c => !string.IsNullOrWhiteSpace(c.TexturePath)))
        {
            _renderer.TrySetChannelTexture(channel.Index, channel.TexturePath, out _);
        }

        var message = _renderer.LastCompileMessage;
        AppendCompileResult(message);
    }

    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        if (_glControl is null || !_glControl.IsHandleCreated)
        {
            return;
        }

        var elapsed = _frameStopwatch.Elapsed.TotalSeconds;
        _frameStopwatch.Restart();

        HandleCameraInput((float)elapsed);
        _glControl.MakeCurrent();
        _renderer.Render(elapsed);
        _glControl.SwapBuffers();

        _framesInWindow++;
        var now = DateTime.UtcNow;
        var dt = (now - _fpsWindowStart).TotalSeconds;
        if (dt >= 1.0)
        {
            FpsTextBlock.Text = $"FPS: {(int)(_framesInWindow / dt)}";
            _fpsWindowStart = now;
            _framesInWindow = 0;
        }
    }

    private void CompileCurrentShader()
    {
        if (_glControl is null)
        {
            return;
        }

        _document.FragmentSource = EditorTextBox.Text;
        _document.VertexSource = VertexEditorTextBox.Text;
        _glControl.MakeCurrent();

        var result = _renderer.Compile(_document.FragmentSource, _document.VertexSource);
        AppendCompileResult(result);

        _sessionStore.Save(_document);
    }

    private void AppendCompileResult(ShaderCompileMessage message)
    {
        var prefix = message.Success ? "[OK]" : "[ERR]";
        var stage = string.IsNullOrWhiteSpace(message.Stage) ? string.Empty : $"[{message.Stage}]";
        var line = message.Line.HasValue ? $" line {message.Line.Value}" : string.Empty;
        AppendDiagnostic($"{prefix}{stage}{line} {message.Message}");
        StatusTextBlock.Text = message.Success ? "Shader compiled" : "Shader error";
    }

    private void AppendDiagnostic(string text)
    {
        DiagnosticsTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        DiagnosticsTextBox.ScrollToEnd();
    }

    private void EditorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (AutoCompileCheckBox.IsChecked != true)
        {
            return;
        }

        _compileDebounceTimer.Stop();
        _compileDebounceTimer.Start();
    }

    private void CompileDebounceTimer_Tick(object? sender, EventArgs e)
    {
        _compileDebounceTimer.Stop();
        CompileCurrentShader();
    }

    private void CompileButton_Click(object sender, RoutedEventArgs e)
    {
        CompileCurrentShader();
    }

    private void AutoCompileCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        _document.AutoCompile = AutoCompileCheckBox.IsChecked == true;
        _sessionStore.Save(_document);
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        _document.IsPaused = !_document.IsPaused;
        _renderer.SetPaused(_document.IsPaused);
        PlayPauseButton.Content = _document.IsPaused ? "Play" : "Pause";
        _sessionStore.Save(_document);
    }

    private void ResetTimeButton_Click(object sender, RoutedEventArgs e)
    {
        _renderer.ResetTime();
        AppendDiagnostic("Time reset.");
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullscreen();
        _document.IsFullscreen = _isFullscreen;
        _sessionStore.Save(_document);
    }

    private void ExitFullscreenIfActive()
    {
        if (!_isFullscreen)
        {
            return;
        }

        ToggleFullscreen();
        _document.IsFullscreen = _isFullscreen;
        _sessionStore.Save(_document);
    }

    private void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResizeMode = ResizeMode;
            _previousTopmost = Topmost;
            _previousEditorColumnWidth = EditorColumnDefinition.Width;
            _previousPreviewColumnWidth = PreviewColumnDefinition.Width;
            _previousDiagnosticsRowHeight = DiagnosticsRowDefinition.Height;
            _previousPreviewHeaderRowHeight = PreviewHeaderRowDefinition.Height;
            _previousPreviewChannelsRowHeight = PreviewChannelsRowDefinition.Height;
            _previousPreviewBorderMargin = PreviewBorder.Margin;

            MainToolBar.Visibility = Visibility.Collapsed;
            MainStatusBar.Visibility = Visibility.Collapsed;
            EditorPaneGrid.Visibility = Visibility.Collapsed;
            DiagnosticsGroupBox.Visibility = Visibility.Collapsed;
            PreviewTitleTextBlock.Visibility = Visibility.Collapsed;
            ChannelButtonsGrid.Visibility = Visibility.Collapsed;

            EditorColumnDefinition.Width = new GridLength(0);
            PreviewColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
            DiagnosticsRowDefinition.Height = new GridLength(0);
            PreviewHeaderRowDefinition.Height = new GridLength(0);
            PreviewChannelsRowDefinition.Height = new GridLength(0);
            PreviewBorder.Margin = new Thickness(0);

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            Topmost = true;
            _isFullscreen = true;
            FullscreenButton.Content = "Windowed";
            return;
        }

        Topmost = _previousTopmost;
        WindowStyle = _previousWindowStyle;
        ResizeMode = _previousResizeMode;
        WindowState = _previousWindowState;

        MainToolBar.Visibility = Visibility.Visible;
        MainStatusBar.Visibility = Visibility.Visible;
        EditorPaneGrid.Visibility = Visibility.Visible;
        DiagnosticsGroupBox.Visibility = Visibility.Visible;
        PreviewTitleTextBlock.Visibility = Visibility.Visible;
        ChannelButtonsGrid.Visibility = Visibility.Visible;

        EditorColumnDefinition.Width = _previousEditorColumnWidth;
        PreviewColumnDefinition.Width = _previousPreviewColumnWidth;
        DiagnosticsRowDefinition.Height = _previousDiagnosticsRowHeight;
        PreviewHeaderRowDefinition.Height = _previousPreviewHeaderRowHeight;
        PreviewChannelsRowDefinition.Height = _previousPreviewChannelsRowHeight;
        PreviewBorder.Margin = _previousPreviewBorderMargin;

        _isFullscreen = false;
        FullscreenButton.Content = "Fullscreen";
    }

    private void RenderModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var mode = RenderModeComboBox.SelectedIndex == 1 ? RenderMode.ThreeD : RenderMode.TwoD;
        _document.RenderMode = mode;
        _document.VertexSource = VertexEditorTextBox.Text;
        if (_glControl is null)
        {
            Update3DControlsState();
            _sessionStore.Save(_document);
            return;
        }

        var result = _renderer.SetRenderMode(mode, _document.VertexSource);
        AppendCompileResult(result);
        Update3DControlsState();

        if (mode == RenderMode.ThreeD)
        {
            if (ModelComboBox.SelectedIndex >= 0)
            {
                TryLoadModelAt(ModelComboBox.SelectedIndex, persistSelection: false);
            }
            AppendDiagnostic("3D mode active. Use WASD + right mouse drag + wheel.");
        }

        _sessionStore.Save(_document);
    }

    private void TemplateComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded || TemplateComboBox.SelectedItem is not string selectedName)
        {
            return;
        }

        var template = ShaderTemplateCatalog.Templates.FirstOrDefault(t => t.Name == selectedName);
        if (template is null)
        {
            return;
        }

        EditorTextBox.Text = template.FragmentSource;
        CompileCurrentShader();
        AppendDiagnostic($"Loaded template: {selectedName}");
    }

    private void LoadChannel0_Click(object sender, RoutedEventArgs e) => LoadChannel(0);
    private void LoadChannel1_Click(object sender, RoutedEventArgs e) => LoadChannel(1);
    private void LoadChannel2_Click(object sender, RoutedEventArgs e) => LoadChannel(2);
    private void LoadChannel3_Click(object sender, RoutedEventArgs e) => LoadChannel(3);
    private void ResetCameraButton_Click(object sender, RoutedEventArgs e)
    {
        _renderer.ResetCamera();
        AppendDiagnostic("Camera reset.");
    }

    private void LoadChannel(int channel)
    {
        if (_glControl is null)
        {
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tga",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _glControl.MakeCurrent();
        var ok = _renderer.TrySetChannelTexture(channel, dialog.FileName, out var message);
        AppendDiagnostic(message);

        if (ok)
        {
            var entry = _document.Channels.First(c => c.Index == channel);
            entry.TexturePath = dialog.FileName;
            _sessionStore.Save(_document);
        }
    }

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            ExitFullscreenIfActive();
            e.Handled = true;
            return;
        }

        if (e.Key == System.Windows.Input.Key.S &&
            (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
        {
            SaveButton_Click(sender, e);
            e.Handled = true;
        }
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Fragment Shaders|*.frag|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        EditorTextBox.Text = File.ReadAllText(dialog.FileName);
        var vertexSidecarPath = GetVertexSidecarPath(dialog.FileName);
        if (File.Exists(vertexSidecarPath))
        {
            VertexEditorTextBox.Text = File.ReadAllText(vertexSidecarPath);
            AppendDiagnostic($"Opened vertex: {vertexSidecarPath}");
        }
        else
        {
            AppendDiagnostic($"Vertex file not found: {vertexSidecarPath}");
        }

        _currentFilePath = dialog.FileName;
        UpdateTitle();
        AppendDiagnostic($"Opened: {dialog.FileName}");
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_currentFilePath))
        {
            SaveToFile(_currentFilePath, confirmOverwrite: true);
        }
        else
        {
            SaveAsButton_Click(sender, e);
        }
    }

    private void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Fragment Shaders|*.frag|All Files|*.*",
            FileName = string.IsNullOrWhiteSpace(_currentFilePath)
                ? "shader.frag"
                : Path.GetFileName(_currentFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _currentFilePath = dialog.FileName;
        SaveToFile(_currentFilePath);
    }

    private void SaveToFile(string path, bool confirmOverwrite = false)
    {
        if (confirmOverwrite && File.Exists(path))
        {
            var answer = System.Windows.MessageBox.Show(
                this,
                $"O arquivo \"{Path.GetFileName(path)}\" já existe. Deseja sobrescrever?",
                "Confirmar sobrescrita",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                AppendDiagnostic($"Save canceled: {path}");
                StatusTextBlock.Text = "Save canceled";
                return;
            }
        }

        File.WriteAllText(path, EditorTextBox.Text);
        var vertexSidecarPath = GetVertexSidecarPath(path);
        File.WriteAllText(vertexSidecarPath, VertexEditorTextBox.Text);
        UpdateTitle();
        AppendDiagnostic($"Saved: {path}");
        AppendDiagnostic($"Saved vertex: {vertexSidecarPath}");
        StatusTextBlock.Text = "Saved";
    }

    private static string GetVertexSidecarPath(string fragmentPath)
    {
        return Path.ChangeExtension(fragmentPath, ".vert");
    }

    private void UpdateTitle()
    {
        Title = string.IsNullOrWhiteSpace(_currentFilePath)
            ? "GLSL Shader Lab Studio"
            : $"GLSL Shader Lab Studio — {Path.GetFileName(_currentFilePath)}";
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _renderTimer.Stop();
        _compileDebounceTimer.Stop();

        _document.FragmentSource = EditorTextBox.Text;
        _document.VertexSource = VertexEditorTextBox.Text;
        _document.AutoCompile = AutoCompileCheckBox.IsChecked == true;
        _document.IsFullscreen = _isFullscreen;
        _document.SelectedModelPath = _renderer.CurrentModelPath;
        _sessionStore.Save(_document);

        if (_glControl is not null)
        {
            _glControl.MakeCurrent();
            _renderer.Dispose();
            _glControl.Dispose();
        }
    }

    private void PopulateModelsCombo()
    {
        var modelsRoot = ResolveModelsRoot();
        _availableModels = _renderer.DiscoverModels(modelsRoot);
        ModelComboBox.Items.Clear();
        foreach (var model in _availableModels)
        {
            ModelComboBox.Items.Add(model.Name);
        }

        if (_availableModels.Count == 0)
        {
            AppendDiagnostic($"No models found in: {modelsRoot}");
            ModelComboBox.SelectedIndex = -1;
            return;
        }

        var selectedIndex = 0;
        if (!string.IsNullOrWhiteSpace(_document.SelectedModelPath))
        {
            for (var i = 0; i < _availableModels.Count; i++)
            {
                if (string.Equals(_availableModels[i].Path, _document.SelectedModelPath, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        ModelComboBox.SelectedIndex = selectedIndex;
    }

    private string ResolveModelsRoot()
    {
        var directory = AppContext.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(directory, "Mesh");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(directory);
            if (parent is null)
            {
                break;
            }

            directory = parent.FullName;
        }

        var fallback = Path.Combine(Directory.GetCurrentDirectory(), "Mesh");
        return fallback;
    }

    private void TryLoadInitialModel()
    {
        if (_availableModels.Count == 0 || ModelComboBox.SelectedIndex < 0)
        {
            return;
        }

        TryLoadModelAt(ModelComboBox.SelectedIndex, persistSelection: false);
    }

    private void ModelComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded || ModelComboBox.SelectedIndex < 0)
        {
            return;
        }

        TryLoadModelAt(ModelComboBox.SelectedIndex, persistSelection: true);
    }

    private void TryLoadModelAt(int index, bool persistSelection)
    {
        if (_glControl is null || index < 0 || index >= _availableModels.Count)
        {
            return;
        }

        _glControl.MakeCurrent();
        var selectedModel = _availableModels[index];
        if (_renderer.TryLoadModel(selectedModel.Path, out var message))
        {
            _document.SelectedModelPath = selectedModel.Path;
            if (persistSelection)
            {
                _sessionStore.Save(_document);
            }
        }

        AppendDiagnostic(message);
    }

    private void Update3DControlsState()
    {
        var is3d = _document.RenderMode == RenderMode.ThreeD;
        ModelComboBox.IsEnabled = is3d && _availableModels.Count > 0;
        ResetCameraButton.IsEnabled = is3d;
        VertexEditorTab.IsEnabled = is3d;
    }

    private void HandleCameraInput(float deltaSeconds)
    {
        if (_document.RenderMode != RenderMode.ThreeD)
        {
            return;
        }

        float forward = 0f;
        float right = 0f;

        if (Keyboard.IsKeyDown(Key.W)) forward += 1f;
        if (Keyboard.IsKeyDown(Key.S)) forward -= 1f;
        if (Keyboard.IsKeyDown(Key.A)) right -= 1f;
        if (Keyboard.IsKeyDown(Key.D)) right += 1f;

        if (forward != 0f || right != 0f)
        {
            _renderer.MoveCamera(forward, right, deltaSeconds);
        }
    }
}
