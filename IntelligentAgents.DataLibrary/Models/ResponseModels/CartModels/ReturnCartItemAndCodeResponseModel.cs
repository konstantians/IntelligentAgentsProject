namespace IntelligentAgents.DataLibrary.Models.ResponseModels.CartModels;
public class ReturnCartItemAndCodeResponseModel
{
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCartItemAndCodeResponseModel() { }
    public ReturnCartItemAndCodeResponseModel(DataLibraryReturnedCodes libraryReturnedCodes)
    {
        ReturnedCode = libraryReturnedCodes;
    }

}
