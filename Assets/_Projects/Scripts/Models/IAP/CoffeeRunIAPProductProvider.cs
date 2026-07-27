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

        // Release catalog intentionally contains only the non-consumable No Ads product.
        products.Add(new IAPProductInfo(GameConstain.IAPProductId.NoAds, IAPProductType.NonConsumable, "No Ads"));

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
