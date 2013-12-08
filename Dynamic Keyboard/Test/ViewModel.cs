using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using Aviad.WPF.Controls;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.ComponentModel;


namespace Test
{
    public class ViewModel : INotifyPropertyChanged
    {
        private List<string> _WaitMessage = new List<string>() {""};
        public IEnumerable WaitMessage { get { return _WaitMessage; } }
        public static Dictionary<string,List<AutoCompleteEntry>> wordBank = new Dictionary<string,List<AutoCompleteEntry>>();
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
            HashSet<string> words = new HashSet<string>();
            int index = SearchTerm.LastIndexOf(" ");
            string currentWord = (index == -1) ? SearchTerm : SearchTerm.Substring(index + 1);
            if (wordBank.Count == 0)
            {
                using (StreamReader sr = new StreamReader("Config\\AC.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        int ind = line.IndexOf(" ");
                        string word = "";
                        int count = 0;
                        if (ind == -1)
                        {
                            word = line;
                            count = 1;
                        }
                        else
                        {
                            word = line.Substring(0, ind);
                            count = Int32.Parse(line.Substring(ind + 1));
                        }
                        for (int i = word.Length; i > 0; i--)
                        {
                            string key = word.Substring(0, i);
                            key = key.ToLower();
                            if (!wordBank.ContainsKey(key))
                            {
                                wordBank.Add(key, new List<AutoCompleteEntry>());
                            }
                            AutoCompleteEntry ac = new AutoCompleteEntry(word);
                            ac.count = count;
                            wordBank[key].Add(ac);
                            wordBank[key] = wordBank[key].OrderBy(x => x.count).ToList();
                        }
                    }
                }

            }

            
            string lowerCurrentWord = currentWord.ToLower();
            if (wordBank.ContainsKey(lowerCurrentWord.Substring(0, currentWord.Length)))
            {
                foreach (AutoCompleteEntry ace in wordBank[lowerCurrentWord.Substring(0, currentWord.Length)])
                {
                    string preserveCaps = "";
                    if (!words.Contains(ace.value.ToLower()))
                    {
                        words.Add(ace.value.ToLower());
                        int j = 0;
                        for (j = 0; j < currentWord.Length; j++)
                        {
                            if (currentWord[j] == char.ToUpper(currentWord[j]))
                            {
                                preserveCaps += char.ToUpper(ace.value[j]);
                            }
                            else
                                preserveCaps += ace.value[j];
                        }
                        while (j < ace.value.Length)
                        {
                            preserveCaps += ace.value[j];
                            j++;
                        }
                        myList.Add(preserveCaps);
                    }
                }
            }
          
            foreach (XmlNode node in result)
            {
                string preserveCaps = "";
                String str = node.SelectSingleNode("suggestion").Attributes["data"].Value;
                int space = str.IndexOf(" ");
                if (space != -1)
                    str = str.Substring(0, space);
                if (!words.Contains(str.ToLower()))
                {
                    words.Add(str.ToLower());
                    int i = 0;
                    for (i = 0; i < currentWord.Length; i++)
                    {
                        if (currentWord[i] == char.ToUpper(currentWord[i]))
                        {
                            preserveCaps += char.ToUpper(str[i]);
                        }
                        else
                            preserveCaps += str[i];
                    }
                    while(i < str.Length)
                    {
                        preserveCaps += str[i];
                        i++;
                    }
                    myList.Add(preserveCaps);
                }
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
