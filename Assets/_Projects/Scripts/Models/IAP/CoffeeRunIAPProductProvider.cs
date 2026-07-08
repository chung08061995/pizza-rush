using System;
using System.Collections.Generic;
using DraftUtils.IAP;
using UnityEngine;

public class CoffeeRunIAPProductProvider : MonoBehaviour, IIAPProductProvider
{
    public IAPProductInfo[] GetProducts()
    {
        if (DataManager.Instance == null || DataManager.Instance.iapData == null)
        {
            Debug.LogWarning("[CoffeeRunIAPProductProvider] DataManager or iapData is not ready.");
            return Array.Empty<IAPProductInfo>();
        }

        var iapData = DataManager.Instance.iapData;
        var products = new List<IAPProductInfo>();

        foreach (var iap in iapData.singleIaps)
        {
            var id = GetProductId(iap.productId, iap.itemType);
            products.Add(new IAPProductInfo(id, IAPProductType.Consumable, iap.itemType.ToString()));
        }

        AddMultipleProduct(products, iapData.noAds, IAPProductType.NonConsumable, "No Ads");
        AddMultipleProduct(products, iapData.noAdsBundle, IAPProductType.NonConsumable, "No Ads Bundle");
        AddMultipleProduct(products, iapData.smallBundle, IAPProductType.Consumable, "Small Bundle");
        AddMultipleProduct(products, iapData.mediumBundle, IAPProductType.Consumable, "Medium Bundle");
        AddMultipleProduct(products, iapData.largeBundle, IAPProductType.Consumable, "Large Bundle");
        AddMultipleProduct(products, iapData.starter, IAPProductType.Consumable, "Starter Bundle");

        return products.ToArray();
    }

    private static void AddMultipleProduct(
        List<IAPProductInfo> products,
        MultipleIAPData data,
        IAPProductType productType,
        string displayName)
    {
        if (data == null)
        {
            return;
        }

        var id = GetProductId(data.productId, data.itemType);
        products.Add(new IAPProductInfo(id, productType, displayName));
    }

    private static string GetProductId(string productId, ItemType fallbackItemType)
    {
        return string.IsNullOrEmpty(productId) ? fallbackItemType.ToString() : productId;
    }
}
