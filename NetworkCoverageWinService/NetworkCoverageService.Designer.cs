using System;
using System.Diagnostics;

namespace NetworkCoverageWinService
{
    partial class NetworkCoverageService
    {
       
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            this.WriteToFile("In Dispose");
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            this.WriteToFile("End Dispose");
        }

           
        private void InitializeComponent()
        {
            this.WriteToFile("In InitializeComponent");

            components = new System.ComponentModel.Container();
            this.ServiceName = "NetworkCoverageService";
            this.CanPauseAndContinue = true;
            this.WriteToFile("End InitializeComponent");
        }

    }
}
