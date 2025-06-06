# Reusable UI Patterns

## MainWindow
- **Purpose:** Central hub for navigation and patching workflow.
- **Required controls:** menuStrip, toolStrip, navigationPanel, contentPanel, statusStrip.
- **Key layout rule:** Use TableLayoutPanel with 16px padding; dock panels to fill and start centered.

## GridView
- **Purpose:** Display searchable lists of files or results in table form.
- **Required controls:** dataGridView, searchBox, refreshButton, paginationControls.
- **Key layout rule:** DataGridView fills container; filter controls dock top with 16px spacing.

## DataEntry
- **Purpose:** Input form for specifying patch details or configuration.
- **Required controls:** labels, textBoxes, comboBoxes, checkBoxes, saveButton, cancelButton.
- **Key layout rule:** Arrange labels and inputs in TableLayoutPanel rows with 16px margins.

## Settings
- **Purpose:** Adjust preferences and module options.
- **Required controls:** tabControl, checkBoxes, radioButtons, applyButton, resetButton.
- **Key layout rule:** Each tab uses FlowLayoutPanel "cards" with consistent 16px spacing.

## ModalDialog
- **Purpose:** Confirm actions or display critical messages.
- **Required controls:** label, okButton, cancelButton.
- **Key layout rule:** Center on screen; content panel padded 16px with buttons bottom-right in FlowLayoutPanel.
