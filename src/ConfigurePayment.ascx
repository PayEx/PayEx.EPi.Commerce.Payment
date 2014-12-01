<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Epinova.PayExProvider.ConfigurePayment" %>
<div id="DataForm">
    <table cellpadding="0" cellspacing="2">
	    <tr>
		    <td class="FormLabelCell" colspan="2"><b><asp:Literal runat="server" Text="Configure PayEx Payment" /></b></td>
	    </tr>
    </table>
    <br />
    <table class="DataForm">
	    <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="PayEx PriceArgList" />:</td>
		    <td class="FormFieldCell"><asp:TextBox runat="server" ID="PriceArgList" Width="230"></asp:TextBox></td>
	    </tr>
	    <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="VAT" />:</td>
		    <td class="FormFieldCell"><asp:TextBox runat="server" ID="VAT" Width="230"></asp:TextBox><br>
			    <asp:RequiredFieldValidator ControlToValidate="VAT" Display="dynamic" Font-Name="verdana" Font-Size="9pt" ErrorMessage="VAT is required"
				    runat="server"></asp:RequiredFieldValidator>
		    </td>
	    </tr>
            <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
		    <td class="FormLabelCell"><asp:Literal runat="server" Text="Purchase operation (SALE/AUTHORIZATION)" />:</td>
		    <td class="FormFieldCell"><asp:TextBox runat="server" ID="PurchaseOperation" Width="230"></asp:TextBox><br>
			    <asp:RequiredFieldValidator ControlToValidate="PurchaseOperation" Display="dynamic" Font-Name="verdana" Font-Size="9pt" ErrorMessage="PurchaseOperation is required"
				    runat="server"></asp:RequiredFieldValidator>
		    </td>
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
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
              <td class="FormLabelCell"><asp:Literal runat="server" Text="DefaultView" />:</td>
	          <td class="FormFieldCell">
		            <asp:TextBox Runat="server" ID="DefaultView" Width="300px"></asp:TextBox><br>
		            <asp:RequiredFieldValidator ControlToValidate="DefaultView" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
			                ErrorMessage="DefaultView is required" runat="server"></asp:RequiredFieldValidator>
	          </td>
        </tr>
    </table>
</div>