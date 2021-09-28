using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FrequencyDictionary
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Regex _uriRegEx = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");

        private const string _errorMessage = "Something went wrong. Please check validity of URI and whether you are connected to the internet, then try again";

        private const string OkMessage = "OK";

        private const string dictionaryKeyValueSeparator = ": ";

        private const string dictionaryPairSeparator = "\n";

        public MainWindow()
        {
            InitializeComponent();
            ParseButton.IsEnabled = false;
        }

        private async void URITextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValidURI = await IsUriValid(URITextBox.Text);
            HighlightValidity(isValidURI);
            ToggleButton(isValidURI);
        }

        private void HighlightValidity(bool isValidURI)
        {
            URITextBox.BorderBrush = (isValidURI) ? (Brushes.LightGreen) : (Brushes.IndianRed);
        }

        private void ToggleButton(bool validity)
        {
            ParseButton.IsEnabled = validity;
        }

        private async Task<bool> IsUriValid(string uri)
        {
            return await Task.Run(() => _uriRegEx.IsMatch(uri));
        }


        private async Task<HttpWebResponse> EstablishConnection(WebRequest request)
        {
            try
            {
                return await Task.Run(
                    () =>
                        (HttpWebResponse)request.GetResponse()
                        );


                    }
            catch (Exception)
            {
                Status.Text = _errorMessage;
                return null;
            }

        }


        private WebRequest GetRequest()
        {
            WebRequest request = WebRequest.Create(URITextBox.Text);

            request.Method = "GET";
            request.Timeout = 10000;
            return request;
        }


        private bool ResponseStatusHandling(HttpWebResponse response)
        {
            HttpStatusCode responseStatusCode = response.StatusCode;

            bool responseIsOK = responseStatusCode == HttpStatusCode.OK;

            Status.Text = (responseIsOK) ? (OkMessage) : (_errorMessage);

            return responseIsOK;
        }

        private async void ParseButton_Click(object sender, RoutedEventArgs e)
        {
           


            HttpWebResponse response =
                await EstablishConnection(GetRequest());


            if (response is null) return;
            bool responseIsOK = ResponseStatusHandling(response);



            if (responseIsOK)
            {
                using (
                    StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string contents = await Task.Run(() => reader.ReadToEnd());
                    Dictionary<string, int> frequencyDictionary = await FormDictionary(contents);
                    Dictionary.Text = await Task.Run(() =>
                    {
                        string[] resultArr = frequencyDictionary.Select(pair =>
                       new StringBuilder().Append(pair.Key).Append(dictionaryKeyValueSeparator).Append(pair.Value).ToString()
                            )
                            .ToArray();
                        return string.Join(dictionaryPairSeparator, resultArr);


                    });
                   





                }
            }

    





        }

        private async Task<Dictionary<string, int>> FormDictionary(string contents)
        {
            return await Task.Run(() =>

               contents.Split(' ')
               .GroupBy(word => word)
               .ToDictionary(
                   group => group.Key, group => group.Count()
                   )

                );




        }






    }
}
