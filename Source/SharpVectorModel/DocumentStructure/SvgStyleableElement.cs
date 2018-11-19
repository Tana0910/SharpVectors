using System;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// SvgStyleableElement is an extension to the Svg DOM to create a class for all elements that are styleable.
    /// </summary>
    public abstract class SvgStyleableElement : SvgElement, ISvgStylable
    {
        #region Private static fields

        private static Regex isImportant = new Regex(@"!\s*important$");
        
        #endregion

        #region Private Fields

        private ISvgAnimatedString _className;
        private Dictionary<string, ICssValue> _presentationAttributes = new Dictionary<string, ICssValue>();

        #endregion

        #region Constructors

        protected SvgStyleableElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
        }

        #endregion

        #region ISvgStylable Members

        public ISvgAnimatedString ClassName
        {
            get
            {
                if (_className == null)
                {
                    _className = new SvgAnimatedString(GetAttribute("class", string.Empty));
                }
                return _className;
            }
        }

        public ICssValue GetPresentationAttribute(string name)
        {
            if (!_presentationAttributes.ContainsKey(name))
            {
                ICssValue result;
                string attValue = GetAttribute(name, string.Empty).Trim();
                if (attValue != null && attValue.Length > 0)
                {
                    if (isImportant.IsMatch(attValue))
                    {
                        result = null;
                    }
                    else
                    {
                        result = CssValue.GetCssValue(attValue, false);
                    }
                }
                else
                {
                    result = null;
                }
                _presentationAttributes[name] = result;

            }

            return _presentationAttributes[name];
        }
        
        #endregion

        #region GetValues

        public string GetPropertyValue(string name)
        {
            return GetComputedStyle(string.Empty).GetPropertyValue(name);
        }

        public string GetPropertyValue(string name1, string name2)
        {
            string cssString = GetComputedStyle(string.Empty).GetPropertyValue(name1);
            if (cssString == null)
            {
                cssString = GetComputedStyle(string.Empty).GetPropertyValue(name2);
            }

            return cssString;
        }

        public override ICssStyleDeclaration GetComputedStyle(string pseudoElt)
        {
            if (cachedCSD == null)
            {
                CssCollectedStyleDeclaration csd = (CssCollectedStyleDeclaration)base.GetComputedStyle(pseudoElt);

                var propNames = this.OwnerDocument.CssPropertyProfile.GetAllPropertyNames();

                IEnumerator<string> cssPropNames = propNames.GetEnumerator();
                while (cssPropNames.MoveNext())
                {
                    string cssPropName = cssPropNames.Current;
                    CssValue cssValue = (CssValue)GetPresentationAttribute(cssPropName);
                    if (cssValue != null)
                    {
                        csd.CollectProperty(cssPropName, 0, cssValue,
                            CssStyleSheetType.NonCssPresentationalHints, string.Empty);
                    }
                }

                cachedCSD = csd;
            }
            return cachedCSD;
        }

        #endregion

        #region Update handling

        public override void HandleAttributeChange(XmlAttribute attribute)
        {
            if (attribute.NamespaceURI.Length == 0)
            {
                string localName = attribute.LocalName;
                if (_presentationAttributes.ContainsKey(localName))
                {
                    _presentationAttributes.Remove(localName);
                }

                switch (attribute.LocalName)
                {
                    case "class":
                        _className = null;
                        // class changes need to propagate to children and invalidate CSS
                        break;
                }
            }
            base.HandleAttributeChange(attribute);
        }

        #endregion
    }
}
