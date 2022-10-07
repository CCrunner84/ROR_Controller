using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.RORController;
using ASCOM.Utilities;

namespace ASCOM.RORController
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        TraceLogger tl; // Holder for a reference to the driver's trace logger

        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();

            // Save the provided trace logger for use within the setup dialogue
            tl = tlDriver;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            tl.Enabled = chkTrace.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            chkTrace.Checked = tl.Enabled;

        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {

        }

        private void txtCurrentAzimuth_Click(object sender, EventArgs e)
        {

        }

        private void tmrUpdateStatus_Tick(object sender, EventArgs e)
        {
            if (Dome.bRoofOpened)
                txtRoofOpened.BackColor = Color.LightGreen;
            else
                txtRoofOpened.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bRoofOpening)
                txtRoofOpening.BackColor = Color.LightGreen;
            else
                txtRoofOpening.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bNearOpenPositionInput)
                txtRoofNearOpenPosition.BackColor = Color.LightGreen;
            else
                txtRoofNearOpenPosition.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bRoofClosed)
                txtRoofClosed.BackColor = Color.LightGreen;
            else
                txtRoofClosed.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bRoofClosing)
                txtRoofClosing.BackColor = Color.LightGreen;
            else
                txtRoofClosing.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bNearClosedPositionInput)
                txtRoofNearClosedPosition.BackColor = Color.LightGreen;
            else
                txtRoofNearClosedPosition.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bRoofMoving)
                txtRoofMoving.BackColor = Color.LightGreen;
            else
                txtRoofMoving.BackColor = Color.FromKnownColor(KnownColor.Control);

            if (Dome.bTheSkyXMountConnected)
                txtMountConnected.BackColor = Color.LightGreen;
            else
                txtMountConnected.BackColor = Color.LightSalmon;

            if (Dome.bTheSkyXMountParked)
                txtMountParked.BackColor = Color.LightGreen;
            else
                txtMountParked.BackColor = Color.LightSalmon;
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            
        }
    }
}