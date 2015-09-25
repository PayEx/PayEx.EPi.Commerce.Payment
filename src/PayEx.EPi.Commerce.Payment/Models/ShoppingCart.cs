using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PayEx.EPi.Commerce.Payment.Models
{
    [Serializable()]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ShoppingCart
    {

        private string _currencyCodeField;

        private long _subtotalField;

        private ShoppingCartItem[] _shoppingCartItemField;

        private ExtensionPoint _extensionPointField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string CurrencyCode
        {
            get
            {
                return this._currencyCodeField;
            }
            set
            {
                this._currencyCodeField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public long Subtotal
        {
            get
            {
                return this._subtotalField;
            }
            set
            {
                this._subtotalField = value;
            }
        }

        /// <remarks/>
        [XmlElement("ShoppingCartItem", Form = XmlSchemaForm.Unqualified)]
        public ShoppingCartItem[] ShoppingCartItem
        {
            get
            {
                return this._shoppingCartItemField;
            }
            set
            {
                this._shoppingCartItemField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ExtensionPoint ExtensionPoint
        {
            get
            {
                return this._extensionPointField;
            }
            set
            {
                this._extensionPointField = value;
            }
        }
    }

    [Serializable()]
    [DesignerCategory("code")]
    public partial class ShoppingCartItem
    {

        private string _descriptionField;

        private long _quantityField;

        private long _valueField;

        private string _imageUrlField;

        private ExtensionPoint extensionPointField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Description
        {
            get
            {
                return this._descriptionField;
            }
            set
            {
                this._descriptionField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public long Quantity
        {
            get
            {
                return this._quantityField;
            }
            set
            {
                this._quantityField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public long Value
        {
            get
            {
                return this._valueField;
            }
            set
            {
                this._valueField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ImageURL
        {
            get
            {
                return this._imageUrlField;
            }
            set
            {
                this._imageUrlField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public ExtensionPoint ExtensionPoint
        {
            get
            {
                return this.extensionPointField;
            }
            set
            {
                this.extensionPointField = value;
            }
        }
    }

    [Serializable()]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class ExtensionPoint
    {

        private XmlElement[] _anyField;

        private XmlAttribute[] _anyAttrField;

        /// <remarks/>
        [XmlAnyElement()]
        public XmlElement[] Any
        {
            get
            {
                return this._anyField;
            }
            set
            {
                this._anyField = value;
            }
        }

        /// <remarks/>
        [XmlAnyAttribute()]
        public XmlAttribute[] AnyAttr
        {
            get
            {
                return this._anyAttrField;
            }
            set
            {
                this._anyAttrField = value;
            }
        }
    }
}

