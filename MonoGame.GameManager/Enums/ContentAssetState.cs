namespace MonoGame.GameManager.Enums
{
    public enum ContentAssetState
    {
        /// <summary>
        /// Set as InUse to inform that this asset is used in this screen.
        /// This asset will set automatically NotInUse when the screen is changed.
        /// </summary>
        InUse,
        /// <summary>
        /// Set as NotInUse to unload the asset when change the screen.
        /// </summary>
        NotInUse,
        /// <summary>
        /// Set as fixed to not unload the asset during change screen
        /// </summary>
        Fixed
    }
}
