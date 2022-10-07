
namespace ASCOM.RORController
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.pnlDomeStatus = new System.Windows.Forms.Panel();
            this.txtRoofMoving = new System.Windows.Forms.Label();
            this.txtRoofClosed = new System.Windows.Forms.Label();
            this.txtRoofNearClosedPosition = new System.Windows.Forms.Label();
            this.txtRoofClosing = new System.Windows.Forms.Label();
            this.txtRoofOpened = new System.Windows.Forms.Label();
            this.txtRoofOpening = new System.Windows.Forms.Label();
            this.txtRoofNearOpenPosition = new System.Windows.Forms.Label();
            this.tmrUpdateStatus = new System.Windows.Forms.Timer(this.components);
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.cmdOpen = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtMountConnected = new System.Windows.Forms.Label();
            this.txtMountParked = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.pnlDomeStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(287, 258);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(59, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.RORController.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(298, 9);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(224, 263);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(69, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // pnlDomeStatus
            // 
            this.pnlDomeStatus.Controls.Add(this.txtMountParked);
            this.pnlDomeStatus.Controls.Add(this.txtMountConnected);
            this.pnlDomeStatus.Controls.Add(this.txtRoofMoving);
            this.pnlDomeStatus.Controls.Add(this.txtRoofClosed);
            this.pnlDomeStatus.Controls.Add(this.txtRoofNearClosedPosition);
            this.pnlDomeStatus.Controls.Add(this.txtRoofClosing);
            this.pnlDomeStatus.Controls.Add(this.txtRoofOpened);
            this.pnlDomeStatus.Controls.Add(this.txtRoofOpening);
            this.pnlDomeStatus.Controls.Add(this.txtRoofNearOpenPosition);
            this.pnlDomeStatus.Enabled = false;
            this.pnlDomeStatus.Location = new System.Drawing.Point(12, 12);
            this.pnlDomeStatus.Name = "pnlDomeStatus";
            this.pnlDomeStatus.Size = new System.Drawing.Size(217, 183);
            this.pnlDomeStatus.TabIndex = 20;
            // 
            // txtRoofMoving
            // 
            this.txtRoofMoving.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofMoving.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofMoving.Location = new System.Drawing.Point(3, 127);
            this.txtRoofMoving.Name = "txtRoofMoving";
            this.txtRoofMoving.Size = new System.Drawing.Size(65, 50);
            this.txtRoofMoving.TabIndex = 24;
            this.txtRoofMoving.Text = "Roof Moving";
            this.txtRoofMoving.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofClosed
            // 
            this.txtRoofClosed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofClosed.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofClosed.Location = new System.Drawing.Point(145, 66);
            this.txtRoofClosed.Name = "txtRoofClosed";
            this.txtRoofClosed.Size = new System.Drawing.Size(65, 50);
            this.txtRoofClosed.TabIndex = 23;
            this.txtRoofClosed.Text = "Roof Closed";
            this.txtRoofClosed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofNearClosedPosition
            // 
            this.txtRoofNearClosedPosition.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofNearClosedPosition.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofNearClosedPosition.Location = new System.Drawing.Point(74, 66);
            this.txtRoofNearClosedPosition.Name = "txtRoofNearClosedPosition";
            this.txtRoofNearClosedPosition.Size = new System.Drawing.Size(65, 50);
            this.txtRoofNearClosedPosition.TabIndex = 22;
            this.txtRoofNearClosedPosition.Text = "Roof Near Closed Position";
            this.txtRoofNearClosedPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofClosing
            // 
            this.txtRoofClosing.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofClosing.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofClosing.Location = new System.Drawing.Point(3, 66);
            this.txtRoofClosing.Name = "txtRoofClosing";
            this.txtRoofClosing.Size = new System.Drawing.Size(65, 50);
            this.txtRoofClosing.TabIndex = 21;
            this.txtRoofClosing.Text = "Roof Closing";
            this.txtRoofClosing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofOpened
            // 
            this.txtRoofOpened.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofOpened.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofOpened.Location = new System.Drawing.Point(145, 3);
            this.txtRoofOpened.Name = "txtRoofOpened";
            this.txtRoofOpened.Size = new System.Drawing.Size(65, 50);
            this.txtRoofOpened.TabIndex = 20;
            this.txtRoofOpened.Text = "Roof Opened";
            this.txtRoofOpened.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofOpening
            // 
            this.txtRoofOpening.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofOpening.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofOpening.Location = new System.Drawing.Point(3, 3);
            this.txtRoofOpening.Name = "txtRoofOpening";
            this.txtRoofOpening.Size = new System.Drawing.Size(65, 50);
            this.txtRoofOpening.TabIndex = 13;
            this.txtRoofOpening.Text = "Roof Opening";
            this.txtRoofOpening.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRoofNearOpenPosition
            // 
            this.txtRoofNearOpenPosition.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtRoofNearOpenPosition.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtRoofNearOpenPosition.Location = new System.Drawing.Point(74, 3);
            this.txtRoofNearOpenPosition.Name = "txtRoofNearOpenPosition";
            this.txtRoofNearOpenPosition.Size = new System.Drawing.Size(65, 50);
            this.txtRoofNearOpenPosition.TabIndex = 18;
            this.txtRoofNearOpenPosition.Text = "Roof Near Open Position";
            this.txtRoofNearOpenPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtRoofNearOpenPosition.Click += new System.EventHandler(this.txtCurrentAzimuth_Click);
            // 
            // tmrUpdateStatus
            // 
            this.tmrUpdateStatus.Enabled = true;
            this.tmrUpdateStatus.Interval = 1000;
            this.tmrUpdateStatus.Tick += new System.EventHandler(this.tmrUpdateStatus_Tick);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(287, 228);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(59, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(15, 201);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(65, 23);
            this.cmdConnect.TabIndex = 21;
            this.cmdConnect.Text = "connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // cmdOpen
            // 
            this.cmdOpen.Location = new System.Drawing.Point(86, 201);
            this.cmdOpen.Name = "cmdOpen";
            this.cmdOpen.Size = new System.Drawing.Size(65, 23);
            this.cmdOpen.TabIndex = 22;
            this.cmdOpen.Text = "open";
            this.cmdOpen.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 201);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(65, 23);
            this.button1.TabIndex = 23;
            this.button1.Text = "close";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // txtMountConnected
            // 
            this.txtMountConnected.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtMountConnected.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtMountConnected.Location = new System.Drawing.Point(74, 127);
            this.txtMountConnected.Name = "txtMountConnected";
            this.txtMountConnected.Size = new System.Drawing.Size(65, 50);
            this.txtMountConnected.TabIndex = 25;
            this.txtMountConnected.Text = "Mount Connected";
            this.txtMountConnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMountParked
            // 
            this.txtMountParked.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.txtMountParked.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.txtMountParked.Location = new System.Drawing.Point(145, 127);
            this.txtMountParked.Name = "txtMountParked";
            this.txtMountParked.Size = new System.Drawing.Size(65, 50);
            this.txtMountParked.TabIndex = 26;
            this.txtMountParked.Text = "Mount Parked";
            this.txtMountParked.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 291);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmdOpen);
            this.Controls.Add(this.cmdConnect);
            this.Controls.Add(this.pnlDomeStatus);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RORController Setup";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.pnlDomeStatus.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.Panel pnlDomeStatus;
        private System.Windows.Forms.Label txtRoofMoving;
        private System.Windows.Forms.Label txtRoofClosed;
        private System.Windows.Forms.Label txtRoofClosing;
        private System.Windows.Forms.Label txtRoofOpened;
        private System.Windows.Forms.Label txtRoofOpening;
        private System.Windows.Forms.Label txtRoofNearOpenPosition;
        private System.Windows.Forms.Timer tmrUpdateStatus;
        private System.Windows.Forms.Label txtRoofNearClosedPosition;
        private System.Windows.Forms.Label txtMountParked;
        private System.Windows.Forms.Label txtMountConnected;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Button cmdOpen;
        private System.Windows.Forms.Button button1;
    }
}