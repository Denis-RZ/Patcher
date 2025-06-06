# Windows Forms UI Standards for Universal Code Patcher

## FORM SPECIFICATIONS:
- Default size: 1024x768, MinimumSize: 800x600
- WindowState: Maximized, StartPosition: CenterScreen  
- BackColor: SystemColors.Control (standard Windows gray)
- Font: Segoe UI 9pt for UI, Consolas 9pt for code display

## LAYOUT RULES:
- Use SplitContainer for resizable panels
- Control spacing: 6px between related controls, 12px between groups
- Form padding: 12px from all edges
- Button size: 75x23px (Windows standard), 90x23px for primary actions
- Panel margins: 3px around buttons, 6px around control groups

## CONTROL STANDARDS:
- TreeView: CheckBoxes=true, ShowLines=true, standard Windows appearance
- RichTextBox: Font=Consolas 9pt, ScrollBars=Both, WordWrap=false
- ListView: View=Details, FullRowSelect=true, GridLines=true
- Buttons: FlatStyle=System, UseVisualStyleBackColor=true

## NAMING CONVENTIONS:
- Controls: camelCase + type suffix (fileTreeView, sourceCodeTextBox)
- Event handlers: OnControlAction (OnApplyClick, OnFileSelected)
- Forms: PascalCase + Form suffix (MainForm, SettingsForm)

## VISUAL HIERARCHY:
- MenuStrip and ToolStrip at top
- Main content in SplitContainer layout
- Action buttons bottom-right aligned
- StatusStrip with progress indication at bottom
