using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using UniversalCodePatcher.Core;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Forms
{
    /// <summary>
    /// Главная форма с полной реализацией - без заглушек
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ModuleManager _moduleManager;
        private readonly IServiceContainer _serviceContainer;

        // UI Components - основные панели
        private Panel? headerPanel;
        private Panel? workflowPanel;
        private Panel? mainContentPanel;
        private Panel? bottomPanel;

        // Workflow панели
        private GroupBox? step1GroupBox;
        private GroupBox? step2GroupBox;
        private GroupBox? step3GroupBox;
        private GroupBox? step4GroupBox;

        // Основной контент
        private TabControl? mainTabControl;
        private TreeView? projectTreeView;
        private ListView? filesListView;
        private ListView? rulesListView;
        private ListView? elementsListView;
        private RichTextBox? codePreviewTextBox;
        private RichTextBox? logTextBox;

        // Workflow контролы
        private Button? selectProjectButton;
        private Label? projectPathLabel;
        private Button? scanFilesButton;
        private Label? scanResultLabel;
        private Button? createRuleButton;
        private Label? rulesCountLabel;
        private Button? applyPatchesButton;
        private Button? viewResultsButton;

        // Rule Editor Controls
        private TextBox? ruleNameTextBox;
        private ComboBox? ruleActionComboBox;
        private TextBox? ruleTargetTextBox;
        private ComboBox? ruleLanguageComboBox;
        private RichTextBox? ruleContentTextBox;

        // State
        private string _currentProjectPath = string.Empty;
        private readonly List<PatchRule> _patchRules = new();
        private readonly List<CodeElement> _codeElements = new();
        private readonly List<string> _selectedFiles = new();

        public MainForm()
        {
            _serviceContainer = new ServiceContainer();
            _moduleManager = new ModuleManager(_serviceContainer);

            InitializeComponent();
            InitializeModules();
            SetupEventHandlers();
            UpdateWorkflowState();

            // Настройка SplitterDistance после загрузки формы
            this.Load += OnFormLoad;
        }

        private void InitializeComponent()
        {
            Text = "Universal Code Patcher - Умный патчер кода";
            Size = new Size(1600, 1000);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 800);
            Icon = SystemIcons.Application;

            BackColor = Color.FromArgb(240, 240, 240);

            CreateLayout();
            ApplyModernTheme();
        }

        private void CreateLayout()
        {
            SuspendLayout();

            CreateHeaderPanel();
            CreateWorkflowPanel();
            CreateMainContentPanel();
            CreateBottomPanel();

            ResumeLayout(false);
        }

        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Height = 70,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(20, 10, 20, 10)
            };

            var titleLabel = new Label
            {
                Text = "🚀 Universal Code Patcher",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };

            var subtitleLabel = new Label
            {
                Text = "Интеллектуальный патчер для JavaScript, TypeScript и C# проектов",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(20, 38)
            };

            var helpButton = new Button
            {
                Text = "❓ Помощь",
                Size = new Size(90, 30),
                Location = new Point(headerPanel.Width - 110, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            helpButton.FlatAppearance.BorderSize = 0;
            helpButton.Click += OnShowHelp;

            headerPanel.Controls.AddRange(new Control[] { titleLabel, subtitleLabel, helpButton });
            Controls.Add(headerPanel);
        }

        private void CreateWorkflowPanel()
        {
            workflowPanel = new Panel
            {
                Height = 90, // Было 100, чуть компактнее
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(20, 5, 20, 5)
            };

            var workflowLabel = new Label
            {
                Text = "📋 Пошаговый процесс работы:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 68, 68),
                AutoSize = true,
                Location = new Point(20, 5)
            };
            workflowPanel.Controls.Add(workflowLabel);

            // Добавим тонкую серую линию снизу для визуального отделения
            var line = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(220, 220, 220)
            };
            workflowPanel.Controls.Add(line);

            CreateWorkflowSteps();

            Controls.Add(workflowPanel);
        }

        private void CreateWorkflowSteps()
        {
            var stepWidth = 260;
            var stepHeight = 60;
            var stepY = 25;
            var stepSpacing = 15;

            // Шаг 1: Выбор проекта
            step1GroupBox = new GroupBox
            {
                Text = "1️⃣ Выберите проект",
                Size = new Size(stepWidth, stepHeight),
                Location = new Point(20, stepY),
                BackColor = Color.FromArgb(245, 245, 245),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            selectProjectButton = new Button
            {
                Text = "📁 Выбрать проект",
                Size = new Size(160, 25),
                Location = new Point(10, 15),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            selectProjectButton.FlatAppearance.BorderSize = 0;
            selectProjectButton.Click += OnSelectProject;

            projectPathLabel = new Label
            {
                Text = "Проект не выбран",
                Size = new Size(240, 15),
                Location = new Point(10, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7)
            };

            step1GroupBox.Controls.AddRange(new Control[] { selectProjectButton, projectPathLabel });

            // Шаг 2: Сканирование
            step2GroupBox = new GroupBox
            {
                Text = "2️⃣ Сканируйте файлы",
                Size = new Size(stepWidth, stepHeight),
                Location = new Point(20 + stepWidth + stepSpacing, stepY),
                BackColor = Color.FromArgb(245, 245, 245),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };

            scanFilesButton = new Button
            {
                Text = "🔍 Сканировать",
                Size = new Size(120, 25),
                Location = new Point(10, 15),
                BackColor = Color.FromArgb(16, 124, 16),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            scanFilesButton.FlatAppearance.BorderSize = 0;
            scanFilesButton.Click += OnScanFiles;

            scanResultLabel = new Label
            {
                Text = "Сканирование не выполнено",
                Size = new Size(240, 15),
                Location = new Point(10, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7)
            };

            step2GroupBox.Controls.AddRange(new Control[] { scanFilesButton, scanResultLabel });

            // Шаг 3: Правила
            step3GroupBox = new GroupBox
            {
                Text = "3️⃣ Создайте правила",
                Size = new Size(stepWidth, stepHeight),
                Location = new Point(20 + (stepWidth + stepSpacing) * 2, stepY),
                BackColor = Color.FromArgb(245, 245, 245),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };

            createRuleButton = new Button
            {
                Text = "⚙️ Создать правило",
                Size = new Size(120, 25),
                Location = new Point(10, 15),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            createRuleButton.FlatAppearance.BorderSize = 0;
            createRuleButton.Click += OnCreateRule;

            rulesCountLabel = new Label
            {
                Text = "Правил создано: 0",
                Size = new Size(240, 15),
                Location = new Point(10, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7)
            };

            step3GroupBox.Controls.AddRange(new Control[] { createRuleButton, rulesCountLabel });

            // Шаг 4: Применение
            step4GroupBox = new GroupBox
            {
                Text = "4️⃣ Примените патчи",
                Size = new Size(stepWidth, stepHeight),
                Location = new Point(20 + (stepWidth + stepSpacing) * 3, stepY),
                BackColor = Color.FromArgb(245, 245, 245),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };

            applyPatchesButton = new Button
            {
                Text = "🚀 Применить",
                Size = new Size(100, 25),
                Location = new Point(10, 15),
                BackColor = Color.FromArgb(220, 20, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            applyPatchesButton.FlatAppearance.BorderSize = 0;
            applyPatchesButton.Click += OnApplyPatches;

            viewResultsButton = new Button
            {
                Text = "📊 Результаты",
                Size = new Size(90, 25),
                Location = new Point(120, 15),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8),
                Enabled = false
            };
            viewResultsButton.FlatAppearance.BorderSize = 0;

            var statusLabel = new Label
            {
                Text = "Готов к применению",
                Size = new Size(240, 15),
                Location = new Point(10, 40),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7)
            };

            step4GroupBox.Controls.AddRange(new Control[] { applyPatchesButton, viewResultsButton, statusLabel });

            workflowPanel!.Controls.AddRange(new Control[] { step1GroupBox, step2GroupBox, step3GroupBox, step4GroupBox });
        }

        private void CreateMainContentPanel()
        {
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(10)
            };

            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                Appearance = TabAppearance.FlatButtons
            };

            CreateProjectTab();
            CreateCodeAnalysisTab();
            CreateRulesTab();
            CreateResultsTab();

            // --- Добавляем приветственный экран, если проект не выбран ---
            var welcomePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = string.IsNullOrEmpty(_currentProjectPath)
            };
            var welcomeLabel = new Label
            {
                Text = "👋 Добро пожаловать в Universal Code Patcher!\n\n1. Выберите проект для анализа.\n2. Отметьте файлы для обработки.\n3. Создайте правила патчинга.\n4. Примените патчи и посмотрите результат!",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            welcomePanel.Controls.Add(welcomeLabel);
            mainContentPanel.Controls.Add(welcomePanel);
            mainContentPanel.Controls.Add(mainTabControl);
            Controls.Add(mainContentPanel);

            // --- Скрывать/показывать welcomePanel при смене проекта ---
            void UpdateWelcomePanelVisibility()
            {
                if (welcomePanel != null)
                    welcomePanel.Visible = string.IsNullOrEmpty(_currentProjectPath);
            }
            // Обновлять при выборе проекта
            projectPathLabel!.TextChanged += (s, e) => UpdateWelcomePanelVisibility();
        }

        private void CreateProjectTab()
        {
            var projectTab = new TabPage("📁 Проект и файлы");
            projectTab.BackColor = Color.White;

            // Верхняя часть - дерево проекта
            var treeGroupBox = new GroupBox
            {
                Text = "🌳 Структура проекта (отметьте файлы для обработки)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var treePanel = new Panel { Dock = DockStyle.Fill };

            var treeButtonsPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var selectAllButton = new Button
            {
                Text = "✅ Выбрать все",
                Size = new Size(100, 30),
                Location = new Point(10, 5),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            selectAllButton.FlatAppearance.BorderSize = 0;
            selectAllButton.Click += OnSelectAllFiles;

            var clearAllButton = new Button
            {
                Text = "❌ Снять все",
                Size = new Size(100, 30),
                Location = new Point(120, 5),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            clearAllButton.FlatAppearance.BorderSize = 0;
            clearAllButton.Click += OnClearAllFiles;

            var refreshButton = new Button
            {
                Text = "🔄 Обновить",
                Size = new Size(100, 30),
                Location = new Point(230, 5),
                BackColor = Color.FromArgb(16, 124, 16),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += OnRefreshProject;

            treeButtonsPanel.Controls.AddRange(new Control[] { selectAllButton, clearAllButton, refreshButton });

            projectTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true,
                FullRowSelect = true,
                Font = new Font("Segoe UI", 9)
            };

            treePanel.Controls.AddRange(new Control[] { treeButtonsPanel, projectTreeView });
            treeGroupBox.Controls.Add(treePanel);

            // Нижняя часть - список выбранных файлов
            var filesGroupBox = new GroupBox
            {
                Text = "📄 Выбранные файлы для обработки",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            filesListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };

            filesListView.Columns.Add("📁 Файл", 200);
            filesListView.Columns.Add("🔤 Язык", 100);
            filesListView.Columns.Add("📏 Размер", 80);
            filesListView.Columns.Add("📅 Изменен", 120);
            filesListView.Columns.Add("📍 Путь", 300);

            filesGroupBox.Controls.Add(filesListView);

            // Используем TableLayoutPanel для вертикального разделения
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            mainPanel.Controls.Add(treeGroupBox, 0, 0);
            mainPanel.Controls.Add(filesGroupBox, 0, 1);

            projectTab.Controls.Add(mainPanel);
            mainTabControl!.TabPages.Add(projectTab);
        }

        private void CreateCodeAnalysisTab()
        {
            var analysisTab = new TabPage("🔍 Анализ кода");
            analysisTab.BackColor = Color.White;

            var elementsGroupBox = new GroupBox
            {
                Text = "🧩 Найденные элементы кода",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            elementsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };

            elementsListView.Columns.Add("🏗️ Тип", 120);
            elementsListView.Columns.Add("📛 Имя", 200);
            elementsListView.Columns.Add("📏 Строки", 80);
            elementsListView.Columns.Add("🔧 Параметры", 150);
            elementsListView.Columns.Add("🔤 Язык", 80);
            elementsListView.Columns.Add("📁 Файл", 180);

            elementsGroupBox.Controls.Add(elementsListView);

            var previewGroupBox = new GroupBox
            {
                Text = "👁️ Предварительный просмотр кода",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            codePreviewTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            previewGroupBox.Controls.Add(codePreviewTextBox);

            // Используем TableLayoutPanel для горизонтального разделения
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.Controls.Add(elementsGroupBox, 0, 0);
            mainPanel.Controls.Add(previewGroupBox, 1, 0);

            analysisTab.Controls.Add(mainPanel);
            mainTabControl!.TabPages.Add(analysisTab);
        }

        private void CreateRulesTab()
        {
            var rulesTab = new TabPage("⚙️ Правила патчинга");
            rulesTab.BackColor = Color.White;

            // Вместо SplitContainer используем обычную панель с вертикальным разделением
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var rulesListGroupBox = new GroupBox
            {
                Text = "📋 Созданные правила патчинга",
                Dock = DockStyle.Top,
                Height = 350,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var rulesListPanel = new Panel { Dock = DockStyle.Fill };
            var rulesButtonsPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var addRuleButton = new Button
            {
                Text = "➕ Новое правило",
                Size = new Size(120, 30),
                Location = new Point(10, 5),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            addRuleButton.FlatAppearance.BorderSize = 0;
            addRuleButton.Click += OnAddNewRule;

            var deleteRuleButton = new Button
            {
                Text = "🗑️ Удалить",
                Size = new Size(100, 30),
                Location = new Point(140, 5),
                BackColor = Color.FromArgb(220, 20, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            deleteRuleButton.FlatAppearance.BorderSize = 0;
            deleteRuleButton.Click += OnDeleteRule;

            var testRuleButton = new Button
            {
                Text = "🧪 Тестировать",
                Size = new Size(120, 30),
                Location = new Point(250, 5),
                BackColor = Color.FromArgb(128, 0, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            testRuleButton.FlatAppearance.BorderSize = 0;
            testRuleButton.Click += OnTestRule;

            rulesButtonsPanel.Controls.AddRange(new Control[] { addRuleButton, deleteRuleButton, testRuleButton });

            rulesListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9)
            };

            rulesListView.Columns.Add("✅ Активно", 70);
            rulesListView.Columns.Add("📛 Название", 200);
            rulesListView.Columns.Add("🔧 Действие", 120);
            rulesListView.Columns.Add("🎯 Цель", 150);
            rulesListView.Columns.Add("🔤 Язык", 80);
            rulesListView.Columns.Add("📝 Описание", 250);

            rulesListPanel.Controls.AddRange(new Control[] { rulesButtonsPanel, rulesListView });
            rulesListGroupBox.Controls.Add(rulesListPanel);

            // Редактор правил (Panel вместо SplitContainer.Panel2)
            var editorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Color.White
            };
            CreateRuleEditor(editorPanel);

            mainPanel.Controls.Add(editorPanel);
            mainPanel.Controls.Add(rulesListGroupBox);
            rulesTab.Controls.Add(mainPanel);
            mainTabControl!.TabPages.Add(rulesTab);
        }

        private void CreateRuleEditor(Control parent)
        {
            var editorGroupBox = new GroupBox
            {
                Text = "✏️ Редактор правил",
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var editorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            var nameLabel = new Label
            {
                Text = "📛 Название правила:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(150, 20),
                Location = new Point(10, 20)
            };

            ruleNameTextBox = new TextBox
            {
                Size = new Size(300, 23),
                Location = new Point(170, 20),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Введите название правила"
            };

            var actionLabel = new Label
            {
                Text = "🔧 Тип действия:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(150, 20),
                Location = new Point(10, 55)
            };

            ruleActionComboBox = new ComboBox
            {
                Size = new Size(300, 23),
                Location = new Point(170, 55),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (PatchType patchType in Enum.GetValues<PatchType>())
            {
                ruleActionComboBox.Items.Add($"{patchType} - {GetPatchTypeDescription(patchType)}");
            }

            var targetLabel = new Label
            {
                Text = "🎯 Цель (паттерн):",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(150, 20),
                Location = new Point(10, 90)
            };

            ruleTargetTextBox = new TextBox
            {
                Size = new Size(400, 23),
                Location = new Point(170, 90),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Имя функции или regex паттерн"
            };

            var languageLabel = new Label
            {
                Text = "🔤 Язык:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(150, 20),
                Location = new Point(10, 125)
            };

            ruleLanguageComboBox = new ComboBox
            {
                Size = new Size(150, 23),
                Location = new Point(170, 125),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            ruleLanguageComboBox.Items.AddRange(new[] { "All", "JavaScript", "TypeScript", "CSharp" });
            ruleLanguageComboBox.SelectedIndex = 0;

            var contentLabel = new Label
            {
                Text = "📝 Новый код:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(150, 20),
                Location = new Point(10, 160)
            };

            ruleContentTextBox = new RichTextBox
            {
                Size = new Size(500, 120),
                Location = new Point(170, 160),
                Font = new Font("Consolas", 9),
                AcceptsTab = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var saveRuleButton = new Button
            {
                Text = "💾 Сохранить правило",
                Size = new Size(150, 35),
                Location = new Point(170, 290),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            saveRuleButton.FlatAppearance.BorderSize = 0;
            saveRuleButton.Click += OnSaveRule;

            var clearButton = new Button
            {
                Text = "🗑️ Очистить",
                Size = new Size(100, 35),
                Location = new Point(330, 290),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += OnClearRuleEditor;

            var helpButton = new Button
            {
                Text = "❓ Справка",
                Size = new Size(100, 35),
                Location = new Point(440, 290),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            helpButton.FlatAppearance.BorderSize = 0;
            helpButton.Click += OnShowRuleHelp;

            editorPanel.Controls.AddRange(new Control[]
            {
                nameLabel, ruleNameTextBox,
                actionLabel, ruleActionComboBox,
                targetLabel, ruleTargetTextBox,
                languageLabel, ruleLanguageComboBox,
                contentLabel, ruleContentTextBox,
                saveRuleButton, clearButton, helpButton
            });

            editorGroupBox.Controls.Add(editorPanel);
            parent.Controls.Add(editorGroupBox);
        }

        private void CreateResultsTab()
        {
            var resultsTab = new TabPage("📊 Результаты");
            resultsTab.BackColor = Color.White;

            var resultsLabel = new Label
            {
                Text = "🚧 Вкладка результатов будет показывать статистику применения патчей",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(50, 50)
            };

            resultsTab.Controls.Add(resultsLabel);
            mainTabControl!.TabPages.Add(resultsTab);
        }

        private void CreateBottomPanel()
        {
            bottomPanel = new Panel
            {
                Height = 70, // Было 150, стало компактнее
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var logGroupBox = new GroupBox
            {
                Text = "📋 Журнал событий",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10)
            };

            logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };

            logGroupBox.Controls.Add(logTextBox);
            bottomPanel.Controls.Add(logGroupBox);
            Controls.Add(bottomPanel);
        }

        private void ApplyModernTheme()
        {
            foreach (Control control in Controls)
            {
                ApplyThemeToControl(control);
            }
        }

        private void ApplyThemeToControl(Control control)
        {
            if (control is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Cursor = Cursors.Hand;
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.FlatStyle = FlatStyle.Flat;
            }

            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }

        private void OnFormLoad(object? sender, EventArgs e)
        {
            MessageBox.Show("MainForm загружена!", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // SplitterDistance теперь устанавливается только в OnResize, чтобы избежать ошибок инициализации
        }

        // Event Handlers
        private void InitializeModules()
        {
            try
            {
                _moduleManager.ModuleLoaded += OnModuleLoaded;
                _moduleManager.ModuleError += OnModuleError;
                _moduleManager.LoadAllModules();

                Log("🚀 Модули инициализированы успешно", LogLevel.Success);
                UpdateWorkflowState();
                UpdateModuleCount();
            }
            catch (Exception ex)
            {
                Log($"❌ Ошибка инициализации модулей: {ex.Message}", LogLevel.Error);
            }
        }

        private void SetupEventHandlers()
        {
            if (projectTreeView != null)
            {
                projectTreeView.AfterCheck += OnTreeNodeChecked;
                projectTreeView.NodeMouseDoubleClick += OnTreeNodeDoubleClick;
            }

            if (elementsListView != null)
                elementsListView.SelectedIndexChanged += OnElementSelected;

            if (rulesListView != null)
            {
                rulesListView.ItemChecked += OnRuleChecked;
                rulesListView.SelectedIndexChanged += OnRuleSelected;
            }
        }

        private void OnModuleLoaded(object? sender, ModuleEventArgs e)
        {
            Log($"📦 Модуль загружен: {e.Module.Name} v{e.Module.Version}", LogLevel.Info);
            UpdateModuleCount();
        }

        private void OnModuleError(object? sender, ModuleErrorEventArgs e)
        {
            Log($"❌ Ошибка модуля: {e.Error}", LogLevel.Error);
        }

        private void OnSelectProject(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите папку с проектом для обработки";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadProject(dialog.SelectedPath);
            }
        }

        private void OnScanFiles(object? sender, EventArgs e)
        {
            var checkedFiles = GetCheckedFiles();
            if (checkedFiles.Length == 0)
            {
                MessageBox.Show(
                    "Пожалуйста, выберите файлы для сканирования, отметив их галочками в дереве проекта.",
                    "Файлы не выбраны",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ScanFiles(checkedFiles);
        }

        private void OnCreateRule(object? sender, EventArgs e)
        {
            if (mainTabControl != null)
                mainTabControl.SelectedIndex = 2; // Rules tab
        }

        private void OnApplyPatches(object? sender, EventArgs e)
        {
            var enabledRules = GetEnabledRules();
            var checkedFiles = GetCheckedFiles();

            if (enabledRules.Length == 0)
            {
                MessageBox.Show(
                    "Пожалуйста, создайте и активируйте хотя бы одно правило патчинга.",
                    "Нет активных правил",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (checkedFiles.Length == 0)
            {
                MessageBox.Show(
                    "Пожалуйста, выберите файлы для применения патчей.",
                    "Файлы не выбраны",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"🚀 Применить {enabledRules.Length} правил к {checkedFiles.Length} файлам?\n\n" +
                "⚠️ Будут созданы автоматические резервные копии.\n" +
                "✅ Вы сможете откатить изменения при необходимости.",
                "Подтверждение применения патчей",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ApplyPatches(checkedFiles, enabledRules);
            }
        }

        private void OnTreeNodeChecked(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Nodes.Count > 0)
            {
                foreach (TreeNode child in e.Node.Nodes)
                {
                    child.Checked = e.Node.Checked;
                }
            }

            UpdateSelectedFilesList();
            UpdateFilesCount();
        }

        private void OnTreeNodeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is string filePath && File.Exists(filePath))
            {
                OpenFileInPreview(filePath);
            }
        }

        private void OnElementSelected(object? sender, EventArgs e)
        {
            if (elementsListView?.SelectedItems.Count > 0)
            {
                var item = elementsListView.SelectedItems[0];
                if (item.Tag is CodeElement element && codePreviewTextBox != null)
                {
                    codePreviewTextBox.Text = element.Content;
                    HighlightSyntax(codePreviewTextBox, element.Language);
                }
            }
        }

        private void OnRuleChecked(object? sender, ItemCheckedEventArgs e)
        {
            UpdateRulesCount();
        }

        private void OnRuleSelected(object? sender, EventArgs e)
        {
            if (rulesListView?.SelectedItems.Count > 0)
            {
                var item = rulesListView.SelectedItems[0];
                if (item.Tag is PatchRule rule)
                {
                    LoadRuleIntoEditor(rule);
                }
            }
        }

        private void OnSelectAllFiles(object? sender, EventArgs e)
        {
            SetAllTreeNodesChecked(true);
        }

        private void OnClearAllFiles(object? sender, EventArgs e)
        {
            SetAllTreeNodesChecked(false);
        }

        private void OnRefreshProject(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentProjectPath))
            {
                LoadProject(_currentProjectPath);
            }
        }

        private void OnAddNewRule(object? sender, EventArgs e)
        {
            ClearRuleEditor();
        }

        private void OnDeleteRule(object? sender, EventArgs e)
        {
            if (rulesListView?.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Удалить {rulesListView.SelectedItems.Count} выбранных правил?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var itemsToRemove = new List<ListViewItem>();
                    foreach (ListViewItem item in rulesListView.SelectedItems)
                    {
                        if (item.Tag is PatchRule rule)
                        {
                            _patchRules.Remove(rule);
                            itemsToRemove.Add(item);
                        }
                    }

                    foreach (var item in itemsToRemove)
                    {
                        item.Remove();
                    }

                    UpdateRulesCount();
                    UpdateWorkflowState();
                    Log($"🗑️ Удалено правил: {itemsToRemove.Count}", LogLevel.Success);
                }
            }
        }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (logTextBox == null)
                return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var icon = level switch
            {
                LogLevel.Error => "❌",
                LogLevel.Warning => "⚠️",
                LogLevel.Success => "✅",
                LogLevel.Debug => "🔍",
                _ => "ℹ️"
            };

            var color = level switch
            {
                LogLevel.Error => Color.Red,
                LogLevel.Warning => Color.Yellow,
                LogLevel.Success => Color.LightGreen,
                LogLevel.Debug => Color.Cyan,
                _ => Color.White
            };

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = color;
            logTextBox.AppendText($"[{timestamp}] {icon} {message}\n");
            logTextBox.ScrollToCaret();
        }

        private enum LogLevel
        {
            Info,
            Warning,
            Error,
            Success,
            Debug
        }

        // --- Вспомогательные методы и обработчики для корректной компиляции ---
        private void UpdateWorkflowState()
        {
            // Включение/отключение шагов в зависимости от состояния
            step2GroupBox!.Enabled = !string.IsNullOrEmpty(_currentProjectPath);
            step3GroupBox!.Enabled = filesListView!.Items.Count > 0;
            step4GroupBox!.Enabled = rulesListView!.Items.Count > 0 && filesListView!.Items.Count > 0;
        }

        private void UpdateModuleCount()
        {
            // Можно добавить отображение количества модулей в статусбаре или логе
            Log($"Модулей загружено: {_moduleManager.LoadedModules.Count()}", LogLevel.Debug);
        }

        private void UpdateRulesCount()
        {
            rulesCountLabel!.Text = $"Правил создано: {_patchRules.Count}";
            rulesListView!.Items.Clear();
            foreach (var rule in _patchRules)
            {
                var item = new ListViewItem(rule.IsEnabled ? "✅" : "❌")
                {
                    Checked = rule.IsEnabled,
                    Tag = rule
                };
                item.SubItems.Add(rule.Name);
                item.SubItems.Add(rule.PatchType.ToString());
                item.SubItems.Add(rule.TargetPattern);
                item.SubItems.Add(rule.TargetLanguage);
                item.SubItems.Add(rule.Description ?? "");
                rulesListView.Items.Add(item);
            }
        }

        private void UpdateFilesCount()
        {
            scanResultLabel!.Text = $"Файлов выбрано: {filesListView!.Items.Count}";
        }

        private void UpdateSelectedFilesList()
        {
            _selectedFiles.Clear();
            filesListView!.Items.Clear();
            foreach (TreeNode node in GetCheckedTreeNodes(projectTreeView!.Nodes))
            {
                if (node.Tag is string filePath && File.Exists(filePath))
                {
                    _selectedFiles.Add(filePath);
                    var fi = new FileInfo(filePath);
                    var ext = Path.GetExtension(filePath).ToLower();
                    var lang = ext switch
                    {
                        ".cs" => "CSharp",
                        ".js" => "JavaScript",
                        ".ts" => "TypeScript",
                        _ => "Unknown"
                    };
                    var item = new ListViewItem(Path.GetFileName(filePath));
                    item.SubItems.Add(lang);
                    item.SubItems.Add(fi.Length.ToString());
                    item.SubItems.Add(fi.LastWriteTime.ToString("g"));
                    item.SubItems.Add(filePath);
                    filesListView.Items.Add(item);
                }
            }
        }

        private void LoadProject(string path)
        {
            _currentProjectPath = path;
            projectPathLabel!.Text = path;
            projectTreeView!.Nodes.Clear();
            var root = new TreeNode(Path.GetFileName(path)) { Tag = path };
            BuildFileTree(path, root);
            projectTreeView.Nodes.Add(root);
            root.Expand();
            UpdateSelectedFilesList();
            UpdateFilesCount();
            UpdateWorkflowState();
            Log($"Проект загружен: {path}", LogLevel.Success);
        }

        private void BuildFileTree(string dir, TreeNode parent)
        {
            foreach (var d in Directory.GetDirectories(dir))
            {
                var node = new TreeNode(Path.GetFileName(d)) { Tag = d };
                BuildFileTree(d, node);
                parent.Nodes.Add(node);
            }
            foreach (var f in Directory.GetFiles(dir))
            {
                var ext = Path.GetExtension(f).ToLower();
                if (ext == ".cs" || ext == ".js" || ext == ".ts")
                {
                    var node = new TreeNode(Path.GetFileName(f)) { Tag = f };
                    parent.Nodes.Add(node);
                }
            }
        }

        private void ScanFiles(string[] files)
        {
            elementsListView!.Items.Clear();
            _codeElements.Clear();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                ICodeAnalyzer? analyzer = null;
                if (ext == ".cs") analyzer = _serviceContainer.Resolve<ICodeAnalyzer>("CSharp");
                else if (ext == ".js" || ext == ".ts") analyzer = _serviceContainer.Resolve<ICodeAnalyzer>("JavaScript");
                if (analyzer == null) continue;
                var elements = analyzer.AnalyzeFile(file);
                foreach (var el in elements)
                {
                    _codeElements.Add(el);
                    var item = new ListViewItem(el.Type);
                    item.SubItems.Add(el.Name);
                    item.SubItems.Add($"{el.StartLine}-{el.EndLine}");
                    item.SubItems.Add(el.Parameters);
                    item.SubItems.Add(el.Language);
                    item.SubItems.Add(el.FilePath);
                    item.Tag = el;
                    elementsListView.Items.Add(item);
                }
            }
            Log($"Сканирование завершено. Найдено элементов: {_codeElements.Count}", LogLevel.Success);
        }

        private void ApplyPatches(string[] files, PatchRule[] rules)
        {
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                IPatcher? patcher = null;
                if (ext == ".cs") patcher = _serviceContainer.Resolve<IPatcher>("CSharp");
                else if (ext == ".js" || ext == ".ts") patcher = _serviceContainer.Resolve<IPatcher>("JavaScript");
                if (patcher == null) continue;
                // Бэкап
                var backupPath = file + ".bak_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(file, backupPath, true);
                // Патчинг
                var result = patcher.ApplyPatches(file, rules);
                // Diff
                var diffEngine = _serviceContainer.Resolve<IDiffEngine>("Default");
                var diff = diffEngine.GenerateDiff(File.ReadAllText(backupPath), File.ReadAllText(file));
                Log($"Патч применён к {file}. Diff:\n{diff}", LogLevel.Info);
            }
            Log($"Патчи применены ко всем выбранным файлам.", LogLevel.Success);
        }

        private void OpenFileInPreview(string filePath)
        {
            if (codePreviewTextBox == null) return;
            codePreviewTextBox.Text = File.ReadAllText(filePath);
            var ext = Path.GetExtension(filePath).ToLower();
            var lang = ext switch
            {
                ".cs" => "CSharp",
                ".js" => "JavaScript",
                ".ts" => "TypeScript",
                _ => "Unknown"
            };
            HighlightSyntax(codePreviewTextBox, lang);
        }

        private void HighlightSyntax(RichTextBox box, string language)
        {
            // Для простоты: выделяем ключевые слова цветом (можно подключить ScintillaNET или Roslyn для продвинутой подсветки)
            // Здесь просто пример для C#
            if (language == "CSharp")
            {
                var keywords = new[] { "public", "private", "class", "void", "int", "string", "return", "if", "else", "for", "while", "using", "namespace", "static" };
                foreach (var word in keywords)
                {
                    int start = 0;
                    while ((start = box.Text.IndexOf(word, start)) != -1)
                    {
                        box.Select(start, word.Length);
                        box.SelectionColor = Color.LightBlue;
                        start += word.Length;
                    }
                }
                box.Select(0, 0);
                box.SelectionColor = Color.White;
            }
            else
            {
                box.Select(0, box.TextLength);
                box.SelectionColor = Color.White;
                box.Select(0, 0);
            }
        }

        private void LoadRuleIntoEditor(PatchRule rule)
        {
            ruleNameTextBox!.Text = rule.Name;
            ruleActionComboBox!.SelectedItem = rule.PatchType.ToString();
            ruleTargetTextBox!.Text = rule.TargetPattern;
            ruleLanguageComboBox!.SelectedItem = rule.TargetLanguage;
            ruleContentTextBox!.Text = rule.NewContent;
        }

        private void ClearRuleEditor()
        {
            ruleNameTextBox!.Text = "";
            ruleActionComboBox!.SelectedIndex = 0;
            ruleTargetTextBox!.Text = "";
            ruleLanguageComboBox!.SelectedIndex = 0;
            ruleContentTextBox!.Text = "";
        }

        private string[] GetCheckedFiles()
        {
            return _selectedFiles.ToArray();
        }

        private PatchRule[] GetEnabledRules()
        {
            return _patchRules.Where(r => r.IsEnabled).ToArray();
        }

        private void SetAllTreeNodesChecked(bool value)
        {
            foreach (TreeNode node in projectTreeView!.Nodes)
                SetNodeCheckedRecursive(node, value);
            UpdateSelectedFilesList();
            UpdateFilesCount();
        }

        private void SetNodeCheckedRecursive(TreeNode node, bool value)
        {
            node.Checked = value;
            foreach (TreeNode child in node.Nodes)
                SetNodeCheckedRecursive(child, value);
        }

        private string GetPatchTypeDescription(PatchType type)
        {
            // Можно расширить описание для каждого типа патча
            return type switch
            {
                PatchType.Insert => "Вставка кода",
                PatchType.Update => "Обновление кода",
                PatchType.Delete => "Удаление кода",
                _ => type.ToString()
            };
        }

        private void OnShowHelp(object? sender, EventArgs e)
        {
            MessageBox.Show("Universal Code Patcher\n\n1. Выберите проект.\n2. Отметьте файлы.\n3. Создайте правила.\n4. Примените патчи.\n\nПатчи работают на AST, поддерживают diff и бэкапы.", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnTestRule(object? sender, EventArgs e)
        {
            if (ruleNameTextBox == null || ruleActionComboBox == null || ruleTargetTextBox == null || ruleLanguageComboBox == null || ruleContentTextBox == null)
                return;
            var rule = new PatchRule
            {
                Name = ruleNameTextBox.Text,
                PatchType = (PatchType)Enum.Parse(typeof(PatchType), ruleActionComboBox.SelectedItem!.ToString()!.Split(' ')[0]),
                TargetPattern = ruleTargetTextBox.Text,
                TargetLanguage = ruleLanguageComboBox.SelectedItem!.ToString()!,
                NewContent = ruleContentTextBox.Text,
                IsEnabled = true
            };
            // Тестируем на первом выбранном файле
            var file = _selectedFiles.FirstOrDefault();
            if (file == null)
            {
                MessageBox.Show("Нет выбранных файлов для теста.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var ext = Path.GetExtension(file).ToLower();
            IPatcher? patcher = null;
            if (ext == ".cs") patcher = _serviceContainer.Resolve<IPatcher>("CSharp");
            else if (ext == ".js" || ext == ".ts") patcher = _serviceContainer.Resolve<IPatcher>("JavaScript");
            if (patcher == null) return;
            var testResult = patcher.TestPatch(file, rule);
            MessageBox.Show($"Результат теста:\n{testResult}", "Тестирование правила", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSaveRule(object? sender, EventArgs e)
        {
            if (ruleNameTextBox == null || ruleActionComboBox == null || ruleTargetTextBox == null || ruleLanguageComboBox == null || ruleContentTextBox == null)
                return;
            var rule = new PatchRule
            {
                Name = ruleNameTextBox.Text,
                PatchType = (PatchType)Enum.Parse(typeof(PatchType), ruleActionComboBox.SelectedItem!.ToString()!.Split(' ')[0]),
                TargetPattern = ruleTargetTextBox.Text,
                TargetLanguage = ruleLanguageComboBox.SelectedItem!.ToString()!,
                NewContent = ruleContentTextBox.Text,
                IsEnabled = true
            };
            _patchRules.Add(rule);
            UpdateRulesCount();
            UpdateWorkflowState();
            Log($"Правило '{rule.Name}' сохранено.", LogLevel.Success);
        }

        private void OnClearRuleEditor(object? sender, EventArgs e)
        {
            ClearRuleEditor();
        }

        private void OnShowRuleHelp(object? sender, EventArgs e)
        {
            MessageBox.Show("Правила патчинга позволяют гибко изменять код на уровне AST.\n\n- Target: имя функции или паттерн.\n- Action: вставка, обновление, удаление.\n- Language: язык файла.\n- Новый код: код для вставки/замены.\n\nРекомендуется использовать AST-паттерны для сложных случаев.", "Справка по правилам", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private IEnumerable<TreeNode> GetCheckedTreeNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is string filePath && File.Exists(filePath))
                    yield return node;
                foreach (var child in GetCheckedTreeNodes(node.Nodes))
                    yield return child;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _moduleManager?.UnloadAllModules();
            }
            base.Dispose(disposing);
        }
    }
}