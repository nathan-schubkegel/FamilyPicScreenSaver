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
      this._textBox = new System.Windows.Forms.TextBox();
      this._label1 = new System.Windows.Forms.Label();
      this._label2 = new System.Windows.Forms.Label();
      this._label3 = new System.Windows.Forms.Label();
      this._okButton = new System.Windows.Forms.Button();
      this._cancelButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // _textBox
      // 
      this._textBox.Location = new System.Drawing.Point(20, 113);
      this._textBox.Name = "_textBox";
      this._textBox.Size = new System.Drawing.Size(190, 20);
      this._textBox.TabIndex = 0;
      // 
      // _label1
      // 
      this._label1.AutoSize = true;
      this._label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this._label1.ForeColor = System.Drawing.Color.Black;
      this._label1.Location = new System.Drawing.Point(15, 13);
      this._label1.Name = "_label1";
      this._label1.Size = new System.Drawing.Size(198, 25);
      this._label1.TabIndex = 1;
      this._label1.Text = "Family Pics Screen Saver";
      // 
      // _label2
      // 
      this._label2.AutoSize = true;
      this._label2.Location = new System.Drawing.Point(21, 47);
      this._label2.Name = "_label2";
      this._label2.Size = new System.Drawing.Size(189, 13);
      this._label2.TabIndex = 2;
      this._label2.Text = "By Daddy Schubkegel (cool guy extraordinaire)";
      // 
      // _label3
      // 
      this._label3.AutoSize = true;
      this._label3.Location = new System.Drawing.Point(21, 97);
      this._label3.Name = "_label3";
      this._label3.Size = new System.Drawing.Size(78, 13);
      this._label3.TabIndex = 3;
      this._label3.Text = "Path to pictures:";
      // 
      // _okButton
      // 
      this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this._okButton.Location = new System.Drawing.Point(24, 156);
      this._okButton.Name = "_okButton";
      this._okButton.Size = new System.Drawing.Size(75, 23);
      this._okButton.TabIndex = 4;
      this._okButton.Text = "OK";
      this._okButton.UseVisualStyleBackColor = true;
      this._okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // _cancelButton
      // 
      this._cancelButton.CausesValidation = false;
      this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cancelButton.Location = new System.Drawing.Point(135, 156);
      this._cancelButton.Name = "_cancelButton";
      this._cancelButton.Size = new System.Drawing.Size(75, 23);
      this._cancelButton.TabIndex = 5;
      this._cancelButton.Text = "Cancel";
      this._cancelButton.UseVisualStyleBackColor = true;
      this._cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // SettingsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(265, 194);
      this.Controls.Add(this._cancelButton);
      this.Controls.Add(this._okButton);
      this.Controls.Add(this._label3);
      this.Controls.Add(this._label2);
      this.Controls.Add(this._label1);
      this.Controls.Add(this._textBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.Name = "SettingsForm";
      this.Text = "Screen Saver Settings";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private System.Windows.Forms.TextBox _textBox;
    private System.Windows.Forms.Label _label1;
    private System.Windows.Forms.Label _label2;
    private System.Windows.Forms.Label _label3;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.Button _cancelButton;
  }
}