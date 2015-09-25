<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FinancingInvoiceConfigurePayment.ascx.cs" Inherits="PayEx.EPi.Commerce.Payment.FinancingInvoiceConfigurePayment" %>
<div id="DataForm">
    <table cellpadding="0" cellspacing="2">
	    <tr>
		    <td class="FormLabelCell" colspan="2"><b><asp:Literal runat="server" Text="Configure PayEx Payment" /></b></td>
	    </tr>
    </table>
    <br />
    <table class="DataForm">
	    <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="Get the customers legal address" />:</td>
		    <td class="FormFieldCell"><asp:CheckBox runat="server" ID="GetLegalAddress"></asp:CheckBox></td>
	    </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
	    <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="Use one-phase transaction" />:</td>
		    <td class="FormFieldCell"><asp:CheckBox runat="server" ID="UseOnePhaseTransaction"></asp:CheckBox></td>
	    </tr>
         <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
	    <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="PayEx PriceArgList" />:</td>
		    <td class="FormFieldCell"><asp:TextBox runat="server" ID="PriceArgList" Width="230"></asp:TextBox></td>
	    </tr>
         <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
              <td class="FormLabelCell"><asp:Literal runat="server" Text="AdditionalValues" />:</td>
	          <td class="FormFieldCell">
		            <asp:TextBox Runat="server" ID="AdditionalValues" Width="300px"></asp:TextBox>
	          </td>
        </tr>
    </table>
</div>