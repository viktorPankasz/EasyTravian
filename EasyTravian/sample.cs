        private bool Navigate(string url)
        {
            int n = 0;
            do
            {
                if (url != null && url.Length > 0)
                {
                    pageLoaded = false;
                    if (url == "back")
                        Globals.Web.GoBack();
                    else
                        if (url.StartsWith("http://"))
                            Globals.Web.Navigate(url);
                        else
                            Globals.Web.Navigate("http://" + Globals.Cfg.Server + "/" + url);
                    if (!WaitForBrowser())
                        Globals.Web.Stop();
                    else
                        return true;
                }
            } while (n++ < 3);

            return false;
        }

        private void Submit(int index)
        {
            if (Globals.Web.Document.Forms[index] != null)
            {
                pageLoaded = false;
                Globals.Web.Document.Forms[index].InvokeMember("Submit");
                WaitForBrowser();
            }
        }

        private void Submit()
        {
            Submit(0);
        }

        public bool WaitForBrowser()
        { 
            //while (web.ReadyState == WebBrowserReadyState.Loading)
            Application.UseWaitCursor = true;
            try
            {
                DateTime kezd = DateTime.Now;
                while (!pageLoaded)
                {
                    Thread.Sleep(0);
                    Application.DoEvents();
                    if (DateTime.Now.Subtract(kezd).TotalSeconds > 20)
                        return false;
                }
                return true;
            }
            finally
            {
                Application.UseWaitCursor = false;
            }
        }

        void Web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //if (xpath.ElementExists("id('login_form')/tbody/tr[1]/td/input"))
            if (!LoggedIn)
            {
                if (tryToLogin)
                {
                    //Globals.Web.Stop();
                    pageLoaded = true;
                    throw new Exception("cancel");
                }
                DoLogin();
                tryToLogin = true;
            }
            else
            {
                pageLoaded = true;
                tryToLogin = false;

                ParseAllInfo();
            }
        }

        void DoLogin()
        {
            if (xpath.SetAttribute(GetHTMLElement(TraviHTMLElementId.LoginUserNameInput), "value", Globals.Cfg.UserName)
                &&
                xpath.SetAttribute(GetHTMLElement(TraviHTMLElementId.LoginPasswordInput), "value", Globals.Cfg.PassWord))
            {
                if (Data.TravianServerVersion == TraviVersion.t40)
                    xpath.SetAttribute("id('login_form')/x:tbody/x:tr[3]/x:td/x:input", "value", "1");
                
                HtmlElement el = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.LoginButton));
                el.InvokeMember("Click");
            }

        }

        public bool LoggedIn
        {
            get { return !xpath.ElementExists(GetHTMLElement(TraviHTMLElementId.LoginUserNameInput)); }
        }

        private void ParseAllInfo()
        {
            Parsexxx();

            switch (Globals.Web.Url.AbsolutePath)
            {
                case "/valami.php":
                    ParseValami();
                    break;
                default:
                    break;
            }
        }


//-----------------------------
                    if (!WaitForBrowser())
                        Globals.Web.Stop();

Navigate("http://" + Globals.Cfg.Server + "/" + url);


            Globals.Web.DocumentCompleted += Web_DocumentCompleted;
