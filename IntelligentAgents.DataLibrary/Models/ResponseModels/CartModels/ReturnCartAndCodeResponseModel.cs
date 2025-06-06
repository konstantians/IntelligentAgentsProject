namespace IntelligentAgents.DataLibrary.Models.ResponseModels.CartModels;
public class ReturnCartAndCodeResponseModel
{
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCartAndCodeResponseModel() { }
    public ReturnCartAndCodeResponseModel(DataLibraryReturnedCodes libraryReturnedCodes)
    {
        ReturnedCode = libraryReturnedCodes;
    }

}
