using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dinhthi01

{
    public class RevitData
    {
        private Application application;
        private UIDocument uiDocument;
        private Document document;
        private Selection selection;
        private Transaction transaction;
        private View activeView;
        private List<Element> instanceElements; // biên1 thành viên khác ... // Định nghĩa các thuộc tính để truy xuất  
        public UIApplication UIApplication
        {
            get;
            set;
        }
        public Application Application
        {
            get
            {
                if (application == null) application = UIApplication.Application;
                return application;
            }
        }
        public UIDocument UIDocument
        {
            get
            {
                if (uiDocument == null) uiDocument = UIApplication.ActiveUIDocument;
                return uiDocument;
            }
        }
        public Document Document
        {
            get
            {
                if (document == null) document = UIDocument.Document;
                return document;
            }
        }
        public Selection Selection
        {
            get
            {
                if (selection == null) selection = UIDocument.Selection;
                return selection;
            }
        }
        public Transaction Transaction
        {
            get
            {
                if (transaction == null) transaction = new Transaction(Document, "Add-in");
                return transaction;
            }
        }
        public View ActiveView
        {
            get
            {
                if (activeView == null) activeView = Document.ActiveView;
                return activeView;
            }
        }
        public List<Element> InstanceElements
        {
            get
            {
                if (instanceElements == null) instanceElements = new FilteredElementCollector(Document).WhereElementIsNotElementType().ToList();
                return instanceElements;
            }
        }
    }
}
