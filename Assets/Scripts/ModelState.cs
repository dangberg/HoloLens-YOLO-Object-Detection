namespace Assets.Scripts
{
    /// <summary>
    ///     Possible states of the model.
    /// </summary>
    public enum ModelState
    {
        PreProcessing,
        Executing,
        ReadOutput,
        PostProcessing,
        Idle
    }
}