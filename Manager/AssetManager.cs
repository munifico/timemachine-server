using System;
using System.Collections.Generic;

namespace TimeMachine.Server
{
    public class AssetManager
    {
        #region Lazy Singleton
        private static readonly Lazy<AssetManager> lazy =
            new Lazy<AssetManager>(() => new AssetManager());

        public static AssetManager Instance => lazy.Value;
        #endregion

        private Dictionary<string, string> _assets = new Dictionary<string, string>();

        public void AddAsset(string assetCode, string assetName)
        {
            if (!_assets.ContainsKey(assetCode))
            {
                _assets.Add(assetCode, assetName);
            }
        }

        public string GetAssetName(string assetCode)
        {
            if (_assets.ContainsKey(assetCode))
            {
                return _assets[assetCode];
            }

            throw new InvalidAssetCodeException(assetCode);
        }
    }
}
