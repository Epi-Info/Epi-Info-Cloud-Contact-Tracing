using System.Xml.Linq;

namespace MvcDynamicForms.Fields
{
    public static class Helpers
    {
        //check if the control should be hidden
        public static bool GetControlState(XDocument xdoc, string ControlName, string ListName)
        {

            bool _Val = false;

            if (!string.IsNullOrEmpty(xdoc.ToString()))
            {
                // XDocument xdoc = XDocument.Parse(Xml);

                if (!string.IsNullOrEmpty(xdoc.Root.Attribute(ListName).Value.ToString()))
                {
                    string List = xdoc.Root.Attribute(ListName).Value;
                    string[] ListArray = List.Split(',');
                    for (var i = 0; i < ListArray.Length; i++)
                    {
                        if (ListArray[i] == ControlName.ToLower())
                        {
                            _Val = true;
                            break;
                        }
                        else
                        {

                            _Val = false;
                        }
                    }
                }

            }

            return _Val;
        }
        public static bool GetRequiredControlState(string Requiredlist, string ControlName, string ListName)
        {

            bool _Val = false;

            if (!string.IsNullOrEmpty(Requiredlist))
            {
                if (!string.IsNullOrEmpty(Requiredlist))
                {
                    string List = Requiredlist;
                    string[] ListArray = List.Split(',');
                    for (var i = 0; i < ListArray.Length; i++)
                    {
                        if (ListArray[i].ToLower() == ControlName.ToLower())
                        {
                            _Val = true;
                            break;
                        }
                        else
                        {

                            _Val = false;
                        }
                    }
                }

            }

            return _Val;
        }
    }
}