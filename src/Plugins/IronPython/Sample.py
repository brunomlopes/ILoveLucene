import System
clr.AddReference("System.Windows.Forms")

@export(ICommand)
class MustScream(ICommand):
    @property
    def Text(self):
        return "MustScream"

    @property
    def Description(self):
        return "I have no dialog box but I must scream"

    def Act(self):
        System.Windows.Forms.MessageBox.Show("Screaming", "Shout!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning)

@export(IActOnItem)
class ActOnString(BaseActOnTypedItem[str]):
    def ActOn(self, text):
        System.Windows.Forms.MessageBox.Show(text, "Shout!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning)

