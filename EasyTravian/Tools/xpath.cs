using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace EasyTravian
{
    /// <summary>
    /// Ennek seg�ts�g�vel tudunk �tvonal alapj�n navig�lni a html fileokban
    /// </summary>
    class xpath
    {
        /// <summary>
        /// Kimar egy elementet az adott �tvonal alapj�n
        /// </summary>
        /// <param name="doc">Ebben a doc-ban kell keresni</param>
        /// <param name="path">Ijen �tvonalon</param>
        /// <returns></returns>
        public static HtmlElement SelectElement( string path )
        {
            HtmlElement element = null;

            string[] ss = path.Split('/');
            foreach (string str in ss)
            {
                if (str.StartsWith("id('"))
                    element = Globals.Web.Document.GetElementById(str.Substring(4, str.Length - 6));
                else
                {
                    if (element == null)
                        return null;

                    int level = 0;
                    string tag = str;

                    if (tag.Contains("x:"))
                    {
                        tag = tag.Substring(tag.IndexOf("x:") + 2, tag.Length - 2);
                    }

                    if (tag.Contains("["))
                    {
                        level = int.Parse(tag.Substring(tag.IndexOf("[") + 1, 1)) - 1;
                        tag = tag.Substring(0, tag.IndexOf("["));
                        //if (level == 1)
                        //    level = 0;
                    }
                    try
                    {
                        //ez bazmeg csak ugyanolyan tag kollekci�t tud felolvasni, ha vegyes, akkor kardj�ba d�l:
                        // element = element.GetElementsByTagName(tag)[level];
                        int i = 0;

                        // while-al az a baj, hogy egy kont�neren bel�l a childek lehetnek null -ok is 
                        // a CRLF-ek vagy sz�k�z�k miatt
                        // azt�n a child.NextSibling m�ris nem m�k�dik :)

                        HtmlElement child;
                        for (int c = 0; c < element.Children.Count; c++)
                        {
                            child = element.Children[c];
                            if ((child != null) && (string.Compare(child.TagName, tag, true) == 0))
                                
                            {
                                if (i == level)
                                {
                                    element = child;
                                    break;
                                }
                                i++;
                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Megmondja van-e ilyen eleme a weboldalnak
        /// </summary>
        /// <param name="doc">Ebben keresi</param>
        /// <param name="path">Ezt az �tvonalat</param>
        /// <returns>Van-e</returns>
        public static bool ElementExists( string path )
        {
            return SelectElement(path) != null;
        }

        /// <summary>
        /// Egy node-nak egy attrib�tum�t �rja
        /// Loginn�l van haszn�lva pl.
        /// </summary>
        /// <param name="doc">html doc</param>
        /// <param name="path">xpath</param>
        /// <param name="attribute">attrib</param>
        /// <param name="value">�rt�k</param>
        /// <returns></returns>
        public static bool SetAttribute( string path, string attribute, string value )
        {
            HtmlElement el = SelectElement(path);
            if (el != null)
            {
                el.SetAttribute(attribute, value);
                return true;
            }
            else
                return false;
        }
    }
}
