# Reusable UI Patterns

## MainWindow
- **Purpose:** Central hub for navigation and patching workflow.
- **Required controls:** menuStrip, toolStrip, navigationPanel, contentPanel, statusStrip.
- **Key layout rule:** Use SplitContainer for resizable panels with 12px padding; dock panels to fill and start centered.

## GridView
- **Purpose:** Display searchable lists of files or results in table form.
- **Required controls:** dataGridView, searchBox, refreshButton, paginationControls.
- **Key layout rule:** DataGridView fills container; filter controls dock top with 6px spacing.

## DataEntry
- **Purpose:** Input form for specifying patch details or configuration.
- **Required controls:** labels, textBoxes, comboBoxes, checkBoxes, saveButton, cancelButton.
- **Key layout rule:** Arrange labels and inputs in TableLayoutPanel rows with 6px spacing and 12px margins between groups.

## Settings
- **Purpose:** Adjust preferences and module options.
- **Required controls:** tabControl, checkBoxes, radioButtons, applyButton, resetButton.
- **Key layout rule:** Each tab uses FlowLayoutPanel groups with 6px spacing and 12px between groups.

## ModalDialog
- **Purpose:** Confirm actions or display critical messages.
- **Required controls:** label, okButton, cancelButton.
- **Key layout rule:** Center on screen; content panel padded 12px with buttons bottom-right in FlowLayoutPanel.
