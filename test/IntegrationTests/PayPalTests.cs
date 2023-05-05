﻿using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using PayPal.Api;
using NUnit.Framework;

namespace IntegrationTests;

#if !DEBUG
[Ignore("These tests are to be run only in debug mode")] 
#endif
[TestFixture]
public class PayPalTests
{
	[Test]
	public void CreatePayment()
	{
		var apiContext = new APIContext(GetToken());
		var payment = Payment.Create(apiContext, GetSamplePayment());
		
		//TODO fix this
		Payment.Execute(apiContext, payment.id, new PaymentExecution
		{
			payer_id = payment.payer.payer_info.payer_id,
			transactions = payment.transactions
		});
		Assert.AreEqual("created", payment.state);
	}

	private static Payment GetSamplePayment()
	{
		return new Payment
		{
			intent = "sale",
			payer = new Payer
			{
				payment_method = "paypal",
				payer_info = new PayerInfo
				{
					payer_id = "00001"
				}
			},
			transactions = new List<Transaction>
			{
				new Transaction
				{
					description = "Transaction description.",
					invoice_number = "001",
					amount = new Amount
					{
						currency = "GBP",
						total = "100.00",
						details = new Details
						{
							tax = "15",
							shipping = "10",
							subtotal = "75"
						}
					},
					item_list = new ItemList
					{
						items = new List<Item>
						{
							new Item
							{
								name = "Item Name",
								currency = "GBP",
								price = "15",
								quantity = "5",
								sku = "sku"
							}
						}
					}
				}
			},
			redirect_urls = new RedirectUrls
			{
				return_url = "http://mysite.com/return",
				cancel_url = "http://mysite.com/cancel"
			}
		};
	}

	private static string GetToken()
	{
		var path = Directory.GetCurrentDirectory() + @"..\..\..\..\PayPalTests.json";
		var json = File.ReadAllText(path);
		var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
		return new OAuthTokenCredential(config).GetAccessToken();
	}
}