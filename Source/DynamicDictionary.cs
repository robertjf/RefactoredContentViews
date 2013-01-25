using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace Refactored.UmbracoViewCounter
{
    // The class derived from DynamicObject.
    public class DynamicDictionary : DynamicObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // The inner dictionary.
        readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();

        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public Dictionary<string, object> Items
        {
            get
            {
                return dictionary;
            }
        }

        public bool HasProperty(string propertyName)
        {
            return dictionary.Keys.Contains(propertyName);
        }

        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name;

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return dictionary.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            dictionary[binder.Name] = value;

            OnPropertyChanged(binder.Name);

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        public void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }



        public static dynamic XmlToDynamicDictionary(XElement el)
        {
            string name = string.Empty;
            return DynamicDictionary.XmlToDynamicDictionary(el, out name);
        }

        public static dynamic XmlToDynamicDictionary(XElement el, out string name)
        {
            char[] nameArray = el.Name.LocalName.ToCharArray();
            nameArray[0] = char.ToUpper(nameArray[0]);
            name = new string(nameArray);// CultureInfo.CurrentCulture.TextInfo.ToTitleCase(el.Name.LocalName);

            if (!el.HasElements && !el.HasAttributes)
            {
                if (string.IsNullOrEmpty(el.Value))// && string.IsNullOrEmpty(name))
                    return null;// new DynamicDictionary();

                return el.Value; // Simple datatype
            }

            dynamic exp = new DynamicDictionary();

            var items = exp.Items as IDictionary<String, Object>;

            foreach (var c in el.Elements())
            {
                string n = string.Empty;
                dynamic dc = XmlToDynamicDictionary(c, out n);
                if (items.ContainsKey(n))
                {
                    if (items[n].GetType() != typeof(List<dynamic>))
                    {
                        var l = new List<dynamic>();
                        l.Add(items[n]);
                        items[n] = l;
                    }

                    ((List<dynamic>)items[n]).Add(dc);
                }
                else
                    exp.Items[n] = dc;
            }

            foreach (var a in el.Attributes())
            {
                nameArray = a.Name.LocalName.ToCharArray();
                nameArray[0] = char.ToLower(nameArray[0]);

                exp.Items[new string(nameArray)] = a.Value;
            }
            return exp;
        }

        public static XElement DynamicDictionaryToXML(String nodeName, dynamic node)
        {
            XElement xmlNode = new XElement(nodeName);

            if (node.GetType() is string)
                return xmlNode;

            foreach (var property in node.Items)
            {
                if (property.Value == null)
                {
                    xmlNode.Add(new XElement(property.Key, string.Empty));
                    continue;
                }
                if (property.Value.GetType() == typeof(DynamicDictionary))
                    xmlNode.Add(DynamicDictionaryToXML(property.Key, property.Value));

                else if (property.Value.GetType() == typeof(List<dynamic>))
                    foreach (var element in (List<dynamic>)property.Value)
                        xmlNode.Add(new XElement(property.Key, element));
                else
                {
                    // If the property name starts with an upper case letter, we have a node, otherwise we have an attribute.
                    char firstLetter = property.Key.ToCharArray()[0];
                    if (char.ToLower(firstLetter) == firstLetter)
                        xmlNode.Add(new XAttribute(property.Key, property.Value));
                    else
                        xmlNode.Add(new XElement(property.Key, property.Value));
                }
            }
            return xmlNode;
        }
    }
}
