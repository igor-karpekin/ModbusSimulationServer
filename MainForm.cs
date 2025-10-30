namespace ModbusSimulationServer
{
    public partial class MainForm : Form
    {
        private Logger? _logger;
        private AppConfiguration? _config;
        private CsvConfiguration? _csvConfig;
        private ModbusServer? _modbusServer;
        private DataPlayback? _dataPlayback;
        private RegisterMap? _registerMap;
        private bool _userScrolledGrid = false;
        private bool _userScrolledLog = false;

        public MainForm()
        {
            InitializeComponent();
            
            // Wire up event handlers
            bttnBrowse.Click += BttnBrowse_Click;
            bttnStart.Click += BttnStart_Click;
            bttnPause.Click += BttnPause_Click;
            
            // Wire up help link handlers
            linkMoreInfo.LinkClicked += (s, e) => OpenUrl("https://cyberia-tech.ch/software/mss/help");
            linkWebsite.LinkClicked += (s, e) => OpenUrl("https://cyberia-tech.ch");
            linkDocumentation.LinkClicked += (s, e) => OpenUrl("https://cyberia-tech.ch/software/mss/documentation");
            linkBuyLicense.LinkClicked += (s, e) => OpenUrl("https://cyberia-tech.ch/software/mss/buy");
            
            // Configure DataGridView
            ConfigureDataGridView();
            
            // Initial button states
            bttnStart.Enabled = false;
            bttnPause.Enabled = false;
        }

        private void ConfigureDataGridView()
        {
            gvData.ReadOnly = true;
            gvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gvData.AllowUserToAddRows = false;
            gvData.AllowUserToDeleteRows = false;
            gvData.AllowUserToResizeRows = false;
            gvData.RowHeadersVisible = true;
            gvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            gvData.MultiSelect = false;
            
            // Track user scroll
            gvData.Scroll += (s, e) => _userScrolledGrid = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Load JSON configuration (for default log file path)
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "app_config.json");
                _config = AppConfiguration.LoadFromFile(configPath);

                // Initialize logger with config file path
                string? logFilePath = GetLogFilePathFromConfig();
                
                _logger = new Logger(lbLog, logFilePath);

                // Test logging with different message types using extension methods
                _logger.LogInfo("Application started");
                _logger.LogDebug($"Configuration loaded from: {configPath}");
                _logger.LogInfo("Main form loaded successfully");
                _logger.LogInfo($"Log file path: {logFilePath ?? "Not configured (UI only)"}");
                _logger.LogDebug("Logger initialized and ready");
                _logger.LogInfo("Please select a CSV configuration file using the Browse button");
            }
            catch (Exception ex)
            {
                // If logger initialization fails, show message box
                if (_logger == null)
                {
                    MessageBox.Show($"Failed to initialize application: {ex.Message}", 
                        "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _logger.LogException(ex, "Failed to initialize application");
                }
            }
        }

        private void BttnBrowse_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger?.LogDebug("Opening file browser for CSV configuration");

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Select Configuration File";
                    openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    // Set initial directory to config folder if it exists
                    string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
                    if (Directory.Exists(configDir))
                    {
                        openFileDialog.InitialDirectory = configDir;
                    }

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        _logger?.LogInfo($"Selected file: {filePath}");
                        
                        LoadCsvConfiguration(filePath);
                    }
                    else
                    {
                        _logger?.LogDebug("File selection cancelled");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error during file selection");
                MessageBox.Show($"Error opening file dialog: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCsvConfiguration(string filePath)
        {
            try
            {
                _logger?.LogInfo($"Loading CSV configuration from: {filePath}");

                // Parse the CSV configuration
                _csvConfig = CsvConfigurationParser.Parse(filePath);

                _logger?.LogInfo("CSV configuration parsed successfully");
                _logger?.LogInfo($"  Version: {_csvConfig.Version}");
                _logger?.LogInfo($"  Port: {_csvConfig.Port}");
                _logger?.LogInfo($"  Loops: {_csvConfig.Loops} {(_csvConfig.Loops == 0 ? "(infinite)" : "")}");
                _logger?.LogInfo($"  Debug: {_csvConfig.Debug}");
                
                if (!string.IsNullOrWhiteSpace(_csvConfig.LogFile))
                {
                    _logger?.LogInfo($"  Log File: {_csvConfig.LogFile}");
                }

                // Update logger debug flag BEFORE logging debug messages
                if (_logger != null)
                {
                    _logger.DebugEnabled = _csvConfig.Debug;
                    _logger.LogInfo($"Debug logging: {(_csvConfig.Debug ? "Enabled" : "Disabled")}");
                }

                // Log additional parameters if any
                if (_csvConfig.AdditionalParameters.Count > 0)
                {
                    _logger?.LogDebug($"Additional parameters found: {_csvConfig.AdditionalParameters.Count}");
                    foreach (var param in _csvConfig.AdditionalParameters)
                    {
                        _logger?.LogDebug($"  {param.Key}: {param.Value}");
                    }
                }

                // Log data section info if present
                if (_csvConfig.DataSection != null)
                {
                    _logger?.LogInfo($"Data section loaded:");
                    _logger?.LogInfo($"  Total columns: {_csvConfig.DataSection.Columns.Count}");
                    _logger?.LogInfo($"  Modbus columns: {_csvConfig.DataSection.ModbusColumns.Count()}");
                    _logger?.LogInfo($"  Data rows: {_csvConfig.DataSection.Rows.Count}");
                    
                    // Log column details
                    _logger?.LogDebug("Column definitions:");
                    foreach (var col in _csvConfig.DataSection.Columns)
                    {
                        if (col.IsIgnored)
                        {
                            _logger?.LogDebug($"  Col {col.ColumnIndex}: Ignored - '{col.Description}'");
                        }
                        else if (col.IsDeltaTime)
                        {
                            _logger?.LogDebug($"  Col {col.ColumnIndex}: DeltaTime - '{col.Description}'");
                        }
                        else if (col.IsRef)
                        {
                            _logger?.LogDebug($"  Col {col.ColumnIndex}: Ref - '{col.Description}'");
                        }
                        else if (col.ModbusAddress != null)
                        {
                            _logger?.LogDebug($"  Col {col.ColumnIndex}: {col.ModbusAddress} - '{col.Description}'");
                        }
                    }
                }

                // Validate configuration
                var validationErrors = _csvConfig.Validate();
                if (validationErrors.Count > 0)
                {
                    _logger?.LogError("Configuration validation failed:");
                    foreach (var error in validationErrors)
                    {
                        _logger?.LogError($"  - {error}");
                    }

                    MessageBox.Show(
                        $"Configuration validation failed:\n\n{string.Join("\n", validationErrors)}", 
                        "Configuration Error", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    return;
                }

                // Update UI
                txtFileName.Text = filePath;

                // Update logger with CSV config settings (if logfile specified)
                if (!string.IsNullOrWhiteSpace(_csvConfig.LogFile))
                {
                    UpdateLoggerWithCsvConfig();
                }

                // Populate DataGridView with data section
                if (_csvConfig.DataSection != null)
                {
                    PopulateDataGridView(_csvConfig.DataSection);
                }

                // Enable Start button now that we have valid config
                bttnStart.Enabled = true;
                bttnStart.Text = "Start";

                _logger?.LogInfo("Configuration loaded and validated successfully");

                // Build success message
                var successMsg = $"Configuration loaded successfully!\n\n" +
                    $"Version: {_csvConfig.Version}\n" +
                    $"Port: {_csvConfig.Port}\n" +
                    $"Loops: {_csvConfig.Loops} {(_csvConfig.Loops == 0 ? "(infinite)" : "")}";

                if (_csvConfig.DataSection != null)
                {
                    successMsg += $"\n\nData Section:\n" +
                        $"  Columns: {_csvConfig.DataSection.Columns.Count}\n" +
                        $"  Modbus Registers: {_csvConfig.DataSection.ModbusColumns.Count()}\n" +
                        $"  Data Rows: {_csvConfig.DataSection.Rows.Count}";
                }

                MessageBox.Show(successMsg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileNotFoundException ex)
            {
                _logger?.LogError($"File not found: {ex.Message}");
                MessageBox.Show($"File not found: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidDataException ex)
            {
                _logger?.LogError($"Invalid configuration file: {ex.Message}");
                MessageBox.Show($"Invalid configuration file:\n\n{ex.Message}", 
                    "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Failed to load configuration");
                MessageBox.Show($"Error loading configuration:\n\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateLoggerWithCsvConfig()
        {
            if (_csvConfig == null || string.IsNullOrWhiteSpace(_csvConfig.LogFile))
                return;

            try
            {
                // Dispose old logger
                _logger?.Dispose();

                // Determine log file path
                string logPath = _csvConfig.LogFile;
                if (!Path.IsPathRooted(logPath))
                {
                    // Relative path - create in exe directory
                    logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logPath);
                }

                // Create new logger with updated path and debug flag
                _logger = new Logger(lbLog, logPath, _csvConfig.Debug);
                _logger.LogInfo("Logger updated with CSV configuration");
                _logger.LogInfo($"New log file path: {logPath}");
                _logger.LogInfo($"Debug logging: {(_csvConfig.Debug ? "Enabled" : "Disabled")}");
            }
            catch (Exception ex)
            {
                // Fallback to logger without file
                _logger = new Logger(lbLog, null, true);
                _logger?.LogException(ex, "Failed to update logger with CSV config log file");
            }
        }

        private void PopulateDataGridView(CsvDataSection dataSection)
        {
            try
            {
                _logger?.LogDebug("Populating DataGridView with data section");

                // Clear existing data
                gvData.Columns.Clear();
                gvData.Rows.Clear();

                // Always add Ref column first (if exists)
                var refColumn = dataSection.Columns.FirstOrDefault(c => c.IsRef);
                if (refColumn != null)
                {
                    var dgvColumn = new DataGridViewTextBoxColumn
                    {
                        Name = $"col_ref",
                        HeaderText = refColumn.Assignment,
                        ToolTipText = refColumn.Description,
                        ReadOnly = true,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    gvData.Columns.Add(dgvColumn);
                }

                // Always add Delay column second (if exists)
                var delayColumn = dataSection.Columns.FirstOrDefault(c => c.IsDeltaTime);
                if (delayColumn != null)
                {
                    var dgvColumn = new DataGridViewTextBoxColumn
                    {
                        Name = $"col_delay",
                        HeaderText = delayColumn.Assignment,
                        ToolTipText = delayColumn.Description,
                        ReadOnly = true,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    gvData.Columns.Add(dgvColumn);
                }

                // Add remaining columns (skip ignored, ref, and delay)
                foreach (var col in dataSection.Columns)
                {
                    // Skip ignored columns (empty assignment)
                    if (col.IsIgnored || string.IsNullOrWhiteSpace(col.Assignment))
                        continue;

                    // Skip ref and delay (already added)
                    if (col.IsRef || col.IsDeltaTime)
                        continue;

                    var dgvColumn = new DataGridViewTextBoxColumn
                    {
                        Name = $"col{col.ColumnIndex}",
                        HeaderText = col.Assignment,  // Use assignment (2nd header line) as column header
                        ToolTipText = col.Description, // Use description (1st header line) as tooltip
                        ReadOnly = true,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };

                    gvData.Columns.Add(dgvColumn);
                }

                // Add data rows
                foreach (var dataRow in dataSection.Rows)
                {
                    var dgvRow = new DataGridViewRow();
                    dgvRow.CreateCells(gvData);

                    // Set row header to row number
                    dgvRow.HeaderCell.Value = (dataRow.RowNumber - dataSection.Rows[0].RowNumber + 1).ToString();

                    int cellIndex = 0;

                    // Populate Ref column (if exists)
                    if (refColumn != null)
                    {
                        dgvRow.Cells[cellIndex].Value = dataRow.Ref ?? "";
                        cellIndex++;
                    }

                    // Populate Delay column (if exists)
                    if (delayColumn != null)
                    {
                        dgvRow.Cells[cellIndex].Value = dataRow.DeltaTime.ToString();
                        cellIndex++;
                    }

                    // Populate remaining columns
                    foreach (var column in dataSection.Columns)
                    {
                        // Skip ignored, ref, and delay columns
                        if (column.IsIgnored || string.IsNullOrWhiteSpace(column.Assignment) || 
                            column.IsRef || column.IsDeltaTime)
                            continue;

                        string cellValue = "";
                        bool hasValue = dataRow.Values.ContainsKey(column.ColumnIndex);

                        if (hasValue)
                        {
                            cellValue = dataRow.Values[column.ColumnIndex];
                        }

                        dgvRow.Cells[cellIndex].Value = cellValue;

                        // Mark cells that inherit values (empty in CSV) with different background
                        if (!hasValue || !dataRow.HasValue.GetValueOrDefault(column.ColumnIndex, false))
                        {
                            dgvRow.Cells[cellIndex].Style.BackColor = Color.LightGray;
                            dgvRow.Cells[cellIndex].ToolTipText = "Value inherited from previous row";
                        }

                        cellIndex++;
                    }

                    gvData.Rows.Add(dgvRow);
                }

                _logger?.LogInfo($"DataGridView populated with {gvData.Rows.Count} rows and {gvData.Columns.Count} columns");
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Failed to populate DataGridView");
            }
        }

        /// <summary>
        /// Gets the log file path from configuration.
        /// Returns null if no log file is configured.
        /// </summary>
        private string? GetLogFilePathFromConfig()
        {
            if (_config == null || string.IsNullOrWhiteSpace(_config.LogFilePath))
            {
                // No log file configured - logging only to UI
                return null;
            }

            // Convert relative paths to absolute paths
            string logPath = _config.LogFilePath;
            if (!Path.IsPathRooted(logPath))
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logPath);
            }

            return logPath;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // Log shutdown and cleanup
            _logger?.LogInfo("Application shutting down");
            
            // Stop playback and server
            _dataPlayback?.Dispose();
            _modbusServer?.Dispose();
            
            _logger?.Dispose();
        }

        private void BttnStart_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_modbusServer?.IsRunning == true)
                {
                    // Stop server
                    StopServer();
                }
                else
                {
                    // Start server
                    StartServer();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error in Start/Stop button handler");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BttnPause_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_dataPlayback == null)
                    return;

                if (_dataPlayback.State == DataPlayback.PlaybackState.Playing)
                {
                    _dataPlayback.Pause();
                    bttnPause.Text = "Resume";
                    _logger?.LogInfo("Playback paused");
                }
                else if (_dataPlayback.State == DataPlayback.PlaybackState.Paused)
                {
                    _dataPlayback.Resume();
                    bttnPause.Text = "Pause";
                    _logger?.LogInfo("Playback resumed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error in Pause/Resume button handler");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartServer()
        {
            if (_csvConfig?.DataSection == null)
            {
                MessageBox.Show("No configuration loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _logger?.LogInfo("Starting Modbus server and playback");

            // Create Modbus server
            _modbusServer = new ModbusServer(_logger);
            _modbusServer.Start(_csvConfig.Port);

            // Create register map wrapping server's buffers
            _registerMap = new RegisterMap(_modbusServer.Server!, unitId: 1);

            // Create playback engine
            _dataPlayback = new DataPlayback(_registerMap, _csvConfig.DataSection, _csvConfig.Loops, _logger);
            
            // Subscribe to row changes
            _dataPlayback.RowChanged += OnPlaybackRowChanged;
            
            _dataPlayback.Start();

            // Update UI
            bttnStart.Text = "Stop";
            bttnPause.Enabled = true;
            bttnPause.Text = "Pause";
            bttnBrowse.Enabled = false; // Don't allow changing config while running

            _logger?.LogInfo("Server started and playback running");
        }

        private void StopServer()
        {
            _logger?.LogInfo("Stopping Modbus server and playback");

            // Stop playback
            _dataPlayback?.Dispose();
            _dataPlayback = null;

            // Stop server
            _modbusServer?.Dispose();
            _modbusServer = null;

            _registerMap = null;

            // Update UI
            bttnStart.Text = "Start";
            bttnPause.Enabled = false;
            bttnPause.Text = "Pause";
            bttnBrowse.Enabled = true;

            _logger?.LogInfo("Server stopped");
        }

        private void OnPlaybackRowChanged(object? sender, int rowIndex)
        {
            // Update on UI thread
            if (gvData.InvokeRequired)
            {
                gvData.Invoke(() => OnPlaybackRowChanged(sender, rowIndex));
                return;
            }

            try
            {
                // Select the current row
                if (rowIndex >= 0 && rowIndex < gvData.Rows.Count)
                {
                    gvData.ClearSelection();
                    gvData.Rows[rowIndex].Selected = true;
                    
                    // Only auto-scroll if selected row was visible before OR user just scrolled to it
                    if (!_userScrolledGrid || IsRowVisible(gvData, rowIndex))
                    {
                        gvData.FirstDisplayedScrollingRowIndex = rowIndex;
                        _userScrolledGrid = false; // Reset flag - we're tracking again
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating grid selection: {ex.Message}");
            }
        }

        private bool IsRowVisible(DataGridView grid, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
                return false;

            int firstVisible = grid.FirstDisplayedScrollingRowIndex;
            int visibleCount = grid.DisplayedRowCount(false);
            
            return rowIndex >= firstVisible && rowIndex < firstVisible + visibleCount;
        }

        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to open URL: {ex.Message}");
                MessageBox.Show($"Failed to open URL: {url}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
