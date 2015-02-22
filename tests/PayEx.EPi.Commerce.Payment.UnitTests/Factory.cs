﻿
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.UnitTests
{
    internal static class Factory
    {
        public static string InitializeResult = "<?xml version=\"1.0\" encoding='utf-8' ?><payex><header name=\"Payex Header v1.0\"><id>4e0fbe36b146452b8ce7b87e6186a3e3</id><date>2010-11-10 06:13:48</date></header><status><code>OK</code><description>OK</description><errorCode>OK</errorCode><paramName /><thirdPartyError /></status><orderRef>b93d29bf7df3424cac23407a987c0be9</orderRef><sessionRef>7807accc0e7e4d069e1d92ec5680d0cc</sessionRef><redirectUrl>https://test-confined.payex.com/PxOrderCC.aspx?orderRef=b93d29bf7df3424cac23407a987c0be9</redirectUrl></payex>";

        public static string InitializeResultError = "<?xml version=\"1.0\" encoding='utf-8' ?><payex><header name=\"Payex Header v1.0\"><id>4e0fbe36b146452b8ce7b87e6186a3e3</id><date>2010-11-10 06:13:48</date></header><status><code>Error_Generic</code><description>Error_Generic</description><errorCode>Error_Generic</errorCode><paramName /><thirdPartyError /></status><orderRef></orderRef><sessionRef>7807accc0e7e4d069e1d92ec5680d0cc</sessionRef><redirectUrl>https://test-confined.payex.com/PxOrderCC.aspx?orderRef=b93d29bf7df3424cac23407a987c0be9</redirectUrl></payex>";

        public static string CompleteResult =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?><payex><header name=\"Payex Header v1.0\"><id>123456789abcdef123456789abcdef12</id><date>2010-09-29 04:54:15</date></header><status><code>OK</code><description>OK</description><errorCode>OK</errorCode><paramName /><thirdPartyError /></status><transactionStatus>3</transactionStatus><orderStatus>0</orderStatus><transactionRef>aaabbbcccdddeeefff11122233344455</transactionRef><transactionNumber>12345678</transactionNumber><orderId>123</orderId><productId>My Product</productId><paymentMethod>MC</paymentMethod><amount>2200</amount><alreadyCompleted>False</alreadyCompleted><clientAccount>0</clientAccount><productNumber>My Product</productNumber><clientGsmNumber /><BankHash>12345612-1234-1111-1234-000000000000</BankHash><AuthenticatedStatus>3DSecure</AuthenticatedStatus><AuthenticatedWith>Y</AuthenticatedWith><fraudData>false</fraudData></payex>";

        public static string CompleteResultError =
           "<?xml version=\"1.0\" encoding=\"utf-8\" ?><payex><header name=\"Payex Header v1.0\"><id>123456789abcdef123456789abcdef12</id><date>2010-09-29 04:54:15</date></header><status><code>InvalidAmount</code><description>InvalidAmount</description><errorCode>OK</errorCode><paramName /><thirdPartyError /></status><transactionStatus>3</transactionStatus><orderStatus>0</orderStatus><transactionRef>aaabbbcccdddeeefff11122233344455</transactionRef><transactionNumber>12345678</transactionNumber><orderId>123</orderId><productId>My Product</productId><paymentMethod>MC</paymentMethod><amount>2200</amount><alreadyCompleted>False</alreadyCompleted><clientAccount>0</clientAccount><productNumber>My Product</productNumber><clientGsmNumber /><BankHash>12345612-1234-1111-1234-000000000000</BankHash><AuthenticatedStatus>3DSecure</AuthenticatedStatus><AuthenticatedWith>Y</AuthenticatedWith><fraudData>false</fraudData><errorDetails><transactionErrorCode>InvalidAmount</transactionErrorCode></errorDetails></payex>";

        public static string TransactionResult = 
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?><payex><header name=\"Payex Header v1.0\"><id>423f0efccfbc4f72bdece635f96feadf</id><date>2014-11-27 12:39:40</date></header><status><code>OK</code><description>OK</description><errorCode>OK</errorCode><paramName /><thirdPartyError /><thirdPartySubError /></status><transactionNumber>4998881</transactionNumber><debitAmount>88800</debitAmount><debitCurrency>NOK</debitCurrency><debitAccountNumber>10000858</debitAccountNumber><creditAmount>88800</creditAmount><creditCurrency>NOK</creditCurrency><creditAccountNumber>60020434</creditAccountNumber><orderId>2935880</orderId><productId>1</productId><orderDescription>Kondomeriet ordre: 2935880</orderDescription><transactionStatus>3</transactionStatus><paymentMethod>VISA</paymentMethod><orderCreated>2014-11-27</orderCreated><Csid>a93b7a51-af11-462e-ac08-1db2a0fb614c</Csid><visa><requestCreated>2014-11-27T12:39:31.78+00:00</requestCreated><originalTransactionNumber>4998881</originalTransactionNumber><merchantId>3ff2e393-2246-4975-ae60-cc918ffeeec8</merchantId><operationType>1</operationType><posMerchantNumber>50000004</posMerchantNumber><csId>00000001-4925-0000-0004-000000000000</csId><maskedCard>49**********0004</maskedCard><panId>00000001-4925-0000-0004-000000000000</panId><cardProduct>VISA</cardProduct><country>NOR</country><cardType>Debit Card</cardType><description>Kondomeriet ordre: 2935880</description><recurring>false</recurring><amount>888.0000</amount><currency>578</currency><transactionType>3DSECURE</transactionType><fraudStatus /><requestStatus>0</requestStatus><travelAccountReference /><requestEciCode>06</requestEciCode><responseCreated>2014-11-27T12:39:34.15+00:00</responseCreated><responseEciCode>06</responseEciCode><posTransactionId>123</posTransactionId><sliCode>4</sliCode><aquirer /><aquirerResponseCode>05</aquirerResponseCode><authorizationId>12345</authorizationId><cavvResultCode>123</cavvResultCode><prc>-1</prc><src>-1</src><responseStatus>0</responseStatus><capturedAmount>0.0000</capturedAmount><creditedAmount>0.0000</creditedAmount><canceledAmount>0.0000</canceledAmount><authenticatedWith>Y</authenticatedWith><authenticatedStatus>3DSecure</authenticatedStatus><panBlackListed>false</panBlackListed></visa><addressDetails><addressDetail><type>1</type><firstName>Karoline</firstName><lastName>Klever</lastName><address1>Gesellsvingen 13</address1><address2 /><address3 /><postNumber>1348</postNumber><city>Rykkinn</city><country>Norway</country><countryCode>NO</countryCode><state /><phone /><email>karolikl@gmail.com</email><gsm>0123456789</gsm></addressDetail><addressDetail><type>2</type><firstName>Karoline</firstName><lastName>Klever</lastName><address1>Gesellsvingen 13</address1><address2 /><address3 /><postNumber>1348</postNumber><city>Rykkinn</city><country>Norway</country><countryCode>NO</countryCode><state /><phone /><email /><gsm /></addressDetail></addressDetails></payex>";

        public static Status CreateStatus(bool success)
        {
            if (success)
                return new Status { Description = "OK", ErrorCode = "OK" };
            return new Status { Description = "NotOK", ErrorCode = "NotOK" };
        }
    }
}
