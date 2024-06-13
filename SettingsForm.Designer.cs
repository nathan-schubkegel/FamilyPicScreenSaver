/*
This is free and unencumbered software released into the public domain under The Unlicense.
You have complete freedom to do anything you want with the software, for any purpose.
Please refer to <http://unlicense.org/>
*/

namespace FamilyPicScreenSaver
{
  partial class SettingsForm
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      _label1 = new System.Windows.Forms.Label();
      _label2 = new System.Windows.Forms.Label();
      _label3 = new System.Windows.Forms.Label();
      _okButton = new System.Windows.Forms.Button();
      _cancelButton = new System.Windows.Forms.Button();
      _addButton = new System.Windows.Forms.Button();
      _pictureFolderPathsTextBox = new System.Windows.Forms.RichTextBox();
      SuspendLayout();
      // 
      // _label1
      // 
      _label1.AutoSize = true;
      _label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      _label1.ForeColor = System.Drawing.Color.Black;
      _label1.Location = new System.Drawing.Point(13, 9);
      _label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      _label1.Name = "_label1";
      _label1.Size = new System.Drawing.Size(258, 25);
      _label1.TabIndex = 1;
      _label1.Text = "Family Pics Screen Saver";
      // 
      // _label2
      // 
      _label2.AutoSize = true;
      _label2.Location = new System.Drawing.Point(13, 34);
      _label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      _label2.Name = "_label2";
      _label2.Size = new System.Drawing.Size(254, 15);
      _label2.TabIndex = 2;
      _label2.Text = "By Daddy Schubkegel (cool guy extraordinaire)";
      // 
      // _label3
      // 
      _label3.AutoSize = true;
      _label3.Location = new System.Drawing.Point(13, 59);
      _label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      _label3.Name = "_label3";
      _label3.Size = new System.Drawing.Size(112, 15);
      _label3.TabIndex = 3;
      _label3.Text = "Picture Folder Paths";
      // 
      // _okButton
      // 
      _okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      _okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      _okButton.Location = new System.Drawing.Point(406, 207);
      _okButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      _okButton.Name = "_okButton";
      _okButton.Size = new System.Drawing.Size(88, 27);
      _okButton.TabIndex = 2;
      _okButton.Text = "&OK";
      _okButton.UseVisualStyleBackColor = true;
      _okButton.Click += okButton_Click;
      // 
      // _cancelButton
      // 
      _cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      _cancelButton.CausesValidation = false;
      _cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      _cancelButton.Location = new System.Drawing.Point(502, 207);
      _cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      _cancelButton.Name = "_cancelButton";
      _cancelButton.Size = new System.Drawing.Size(88, 27);
      _cancelButton.TabIndex = 3;
      _cancelButton.Text = "&Cancel";
      _cancelButton.UseVisualStyleBackColor = true;
      _cancelButton.Click += cancelButton_Click;
      // 
      // _addButton
      // 
      _addButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
      _addButton.Location = new System.Drawing.Point(12, 207);
      _addButton.Name = "_addButton";
      _addButton.Size = new System.Drawing.Size(88, 27);
      _addButton.TabIndex = 1;
      _addButton.Text = "&Add...";
      _addButton.UseVisualStyleBackColor = true;
      _addButton.Click += addButton_Click;
      // 
      // _pictureFolderPathsTextBox
      // 
      _pictureFolderPathsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      _pictureFolderPathsTextBox.Location = new System.Drawing.Point(12, 77);
      _pictureFolderPathsTextBox.Name = "_pictureFolderPathsTextBox";
      _pictureFolderPathsTextBox.Size = new System.Drawing.Size(579, 124);
      _pictureFolderPathsTextBox.TabIndex = 4;
      _pictureFolderPathsTextBox.Text = "";
      // 
      // SettingsForm
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      CancelButton = _cancelButton;
      ClientSize = new System.Drawing.Size(603, 246);
      Controls.Add(_pictureFolderPathsTextBox);
      Controls.Add(_addButton);
      Controls.Add(_cancelButton);
      Controls.Add(_okButton);
      Controls.Add(_label3);
      Controls.Add(_label2);
      Controls.Add(_label1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      MaximizeBox = false;
      Name = "SettingsForm";
      Text = "Screen Saver Settings";
      ResumeLayout(false);
      PerformLayout();
    }

    private System.Windows.Forms.Label _label1;
    private System.Windows.Forms.Label _label2;
    private System.Windows.Forms.Label _label3;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.Button _addButton;
    private System.Windows.Forms.RichTextBox _pictureFolderPathsTextBox;
  }
}