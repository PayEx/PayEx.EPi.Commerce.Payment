using System;
using System.Data;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace PayEx.EPi.Commerce.Payment
{
    public partial class MasterPassConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private const string PriceListArgsParameter = "PriceListArgs";
        private const string AdditionalValuesParameter = "AdditionalValues";
        private const string UseBestPracticeFlowParameter = "UseBestPracticeFlow";
        private const string AddShoppingCartXmlParameter = "AddShoppingCartXml";

        public string ValidationGroup { get; set; }

        public MasterPassConfigurePayment()
        {
            ValidationGroup = string.Empty;
            _paymentMethodDto = null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            BindData();
        }

        public void BindData()
        {
            if (_paymentMethodDto?.PaymentMethodParameter != null)
            {
                PaymentMethodDto.PaymentMethodParameterRow parameterByName = GetParameterByName(PriceListArgsParameter);
                if (parameterByName != null)
                {
                    PriceArgList.Text = parameterByName.Value;
                }

                parameterByName = GetParameterByName(AdditionalValuesParameter);
                if (parameterByName != null)
                {
                    AdditionalValues.Text = parameterByName.Value;
                }

                parameterByName = GetParameterByName(UseBestPracticeFlowParameter);
                if (parameterByName != null)
                {
                    UseBestPracticeFlow.Checked = "true".Equals(parameterByName.Value, StringComparison.InvariantCultureIgnoreCase);
                }

                parameterByName = GetParameterByName(AddShoppingCartXmlParameter);
                if (parameterByName != null)
                {
                    AddShoppingCartXml.Checked = "true".Equals(parameterByName.Value, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            else
            {
                Visible = false;
            }
        }

        public void LoadObject(object dto)
        {
            _paymentMethodDto = dto as PaymentMethodDto;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public void SaveChanges(object dto)
        {
            if (Visible)
            {
                _paymentMethodDto = dto as PaymentMethodDto;
                if (_paymentMethodDto?.PaymentMethodParameter != null)
                {
                    Guid paymentMethodId = Guid.Empty;
                    if (_paymentMethodDto.PaymentMethod.Count > 0)
                    {
                        paymentMethodId = _paymentMethodDto.PaymentMethod[0].PaymentMethodId;
                    }

                    PaymentMethodDto.PaymentMethodParameterRow parameterByName = GetParameterByName(PriceListArgsParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = PriceArgList.Text;
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, PriceListArgsParameter, PriceArgList.Text, paymentMethodId);
                    }

                    parameterByName = GetParameterByName(AdditionalValuesParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = AdditionalValues.Text;
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, AdditionalValuesParameter, AdditionalValues.Text, paymentMethodId);
                    }

                    parameterByName = GetParameterByName(UseBestPracticeFlowParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = UseBestPracticeFlow.Checked.ToString();
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, UseBestPracticeFlowParameter, UseBestPracticeFlow.Checked.ToString(), paymentMethodId);
                    }

                    parameterByName = GetParameterByName(AddShoppingCartXmlParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = AddShoppingCartXml.Checked.ToString();
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, AddShoppingCartXmlParameter, AddShoppingCartXml.Checked.ToString(), paymentMethodId);
                    }
                }
            }

        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            PaymentMethodDto.PaymentMethodParameterRow[] rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])_paymentMethodDto.PaymentMethodParameter.Select(string.Format("Parameter = '{0}'", name));
            if ((rowArray != null) && (rowArray.Length > 0))
            {
                return rowArray[0];
            }
            return null;
        }

        private void CreateParameter(PaymentMethodDto dto, string name, string value, Guid paymentMethodId)
        {
            PaymentMethodDto.PaymentMethodParameterRow row = dto.PaymentMethodParameter.NewPaymentMethodParameterRow();
            row.PaymentMethodId = paymentMethodId;
            row.Parameter = name;
            row.Value = value;
            if (row.RowState == DataRowState.Detached)
            {
                dto.PaymentMethodParameter.Rows.Add(row);
            }
        }
    }
}