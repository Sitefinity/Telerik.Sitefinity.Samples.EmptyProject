namespace Telerik.Sitefinity.Samples.Common
{
    public class ColumnDetails
    {
        private ColumnSpaces columnSpaces;
        private string columnClass;
        private int columnWidthPercentage;
        private string placeholderId;

        public ColumnSpaces ColumnSpaces
        {
            get
            {
                return this.columnSpaces;
            }

            set
            {
                this.columnSpaces = value;
            }
        }

        public string ColumnClass
        {
            get
            {
                return this.columnClass;
            }

            set
            {
                this.columnClass = value;
            }
        }

        public int ColumnWidthPercentage
        {
            get
            {
                return this.columnWidthPercentage;
            }

            set
            {
                this.columnWidthPercentage = value;
            }
        }

        public string PlaceholderId
        {
            get
            {
                return this.placeholderId;
            }

            set
            {
                this.placeholderId = value;
            }
        }
    }
}