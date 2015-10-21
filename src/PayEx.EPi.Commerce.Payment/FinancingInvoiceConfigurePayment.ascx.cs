using System;
using System.Data;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace PayEx.EPi.Commerce.Payment
{
    public partial class FinancingInvoiceConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private PaymentMethodDto _paymentMethodDto;
        private const string PriceListArgsParameter = "PriceListArgs";
        private const string AdditionalValuesParameter = "AdditionalValues";
        private const string UseOnePhaseTransactionParameter = "UseOnePhaseTransaction";
        private const string GetLegalAddressParameter = "GetLegalAddress";
        private const string PaymentMethodCodeParameter = "PaymentMethodCode";

        public string ValidationGroup { get; set; }

        public FinancingInvoiceConfigurePayment()
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
            if ((_paymentMethodDto != null) && (_paymentMethodDto.PaymentMethodParameter != null))
            {
                var parameterByName = GetParameterByName(PriceListArgsParameter);
                if (parameterByName != null)
                {
                    PriceArgList.Text = parameterByName.Value;
                }

                parameterByName = GetParameterByName(AdditionalValuesParameter);
                if (parameterByName != null)
                {
                    AdditionalValues.Text = parameterByName.Value;
                }

                parameterByName = GetParameterByName(UseOnePhaseTransactionParameter);
                if (parameterByName != null)
                {
                    UseOnePhaseTransaction.Checked = "true".Equals(parameterByName.Value, StringComparison.InvariantCultureIgnoreCase);
                }

                parameterByName = GetParameterByName(GetLegalAddressParameter);
                if (parameterByName != null)
                {
                    GetLegalAddress.Checked = "true".Equals(parameterByName.Value, StringComparison.InvariantCultureIgnoreCase);
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
                if ((_paymentMethodDto != null) && (_paymentMethodDto.PaymentMethodParameter != null))
                {
                    var paymentMethodId = Guid.Empty;
                    if (_paymentMethodDto.PaymentMethod.Count > 0)
                    {
                        paymentMethodId = _paymentMethodDto.PaymentMethod[0].PaymentMethodId;
                    }

                    var parameterByName = GetParameterByName(PriceListArgsParameter);
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

                    parameterByName = GetParameterByName(UseOnePhaseTransactionParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = UseOnePhaseTransaction.Checked.ToString();
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, UseOnePhaseTransactionParameter, UseOnePhaseTransaction.Checked.ToString(), paymentMethodId);
                    }

                    parameterByName = GetParameterByName(GetLegalAddressParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = GetLegalAddress.Checked.ToString();
                    }
                    else
                    {
                        CreateParameter(_paymentMethodDto, GetLegalAddressParameter, GetLegalAddress.Checked.ToString(), paymentMethodId);
                    }
                }
            }

        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            var rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])_paymentMethodDto.PaymentMethodParameter.Select(string.Format("Parameter = '{0}'", name));
            if ((rowArray != null) && (rowArray.Length > 0))
            {
                return rowArray[0];
            }
            return null;
        }

        private void CreateParameter(PaymentMethodDto dto, string name, string value, Guid paymentMethodId)
        {
            var row = dto.PaymentMethodParameter.NewPaymentMethodParameterRow();
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