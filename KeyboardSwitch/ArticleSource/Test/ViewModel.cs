using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Xml;
using System.Net;
using System.Diagnostics;

namespace Test
{
    public class ViewModel : INotifyPropertyChanged
    {
        private List<string> _WaitMessage = new List<string>() { "Please Wait..." };
        public IEnumerable WaitMessage { get { return _WaitMessage; } }

        private string _QueryText;
        public string QueryText
        {
            get {
                int index = _QueryText.LastIndexOf(" ");
                if (index == -1)
                    return _QueryText;
                return _QueryText.Substring(index);
            }
            set
            {
                if (_QueryText != value)
                {
                    _QueryText = value;
                    OnPropertyChanged("QueryText");
                    _QueryCollection = null;
                    OnPropertyChanged("QueryCollection");
                    Debug.Print("QueryText: " + value);
                }
            }
        }

        public IEnumerable _QueryCollection = null;
        public IEnumerable QueryCollection
        {
            get
            {
                Debug.Print("---" + _QueryCollection);
                QueryGoogle(QueryText);
                return _QueryCollection;
            }
        }

        private void QueryGoogle(string SearchTerm)
        {
            Debug.Print("Query: " + SearchTerm);
            string sanitized = HttpUtility.HtmlEncode(SearchTerm);
            string url = @"http://google.com/complete/search?output=toolbar&q=" + sanitized;
            WebRequest httpWebRequest = HttpWebRequest.Create(url);
            var webResponse = httpWebRequest.GetResponse();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(webResponse.GetResponseStream());
            var result = xmlDoc.SelectNodes("//CompleteSuggestion");
            List<string> myList = new List<string>();
            foreach (XmlNode node in result)
            {
                String str = node.SelectSingleNode("suggestion").Attributes["data"].Value;
                int space = str.IndexOf(" ");
                if (space != -1)
                    str = str.Substring(0, space);
                if (!myList.Contains(str))
                     myList.Add(str);
            }
            _QueryCollection = myList;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
