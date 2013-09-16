using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.BugTrackerNet
{
    internal sealed class BugTrackerNetProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtConnectionString;
        private ValidatingTextBox txtStatus;
        private TextBox txtReleaseNumberCustomField;

        public BugTrackerNetProviderEditor()
        {
        }

        public override void BindToForm(ProviderBase provider)
        {
            this.EnsureChildControls();

            var btnProvider = (BugTrackerNetProvider)provider;
            this.txtConnectionString.Text = btnProvider.ConnectionString;
            this.txtStatus.Text = btnProvider.ClosedStatusName;
            this.txtReleaseNumberCustomField.Text = btnProvider.ReleaseNumberCustomField;
        }

        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new BugTrackerNetProvider
            {
                ConnectionString = this.txtConnectionString.Text,
                ClosedStatusName = this.txtStatus.Text,
                ReleaseNumberCustomField = this.txtReleaseNumberCustomField.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtConnectionString = new ValidatingTextBox { Required = true };
            this.txtStatus = new ValidatingTextBox { Required = true };
            this.txtReleaseNumberCustomField = new TextBox();

            this.Controls.Add(
                new FormFieldGroup("Connection",
                    "A SQL Connection string used to connect to BTN's SQL Database.",
                    false,
                    new StandardFormField("Connection String:", txtConnectionString)
                ),
                new FormFieldGroup("Configuration",
                    "When an issue's status is equal to the 'Closed Status', that issue will be considered closed."
                    + "<br /><br />The Release Number Field is a custom BTN field that ties to the BuildMaster release number.",
                    false,
                    new StandardFormField("Release Number Field:", txtReleaseNumberCustomField),
                    new StandardFormField("Closed Status:", txtStatus)
                )
            );
        }
    }
}
