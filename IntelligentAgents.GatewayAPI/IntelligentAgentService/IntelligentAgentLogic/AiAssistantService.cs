using IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic.Connectors;

namespace IntelligentAgents.GatewayAPI.IntelligentAgentService.IntelligentAgentLogic;

public class AiAssistantService : IAiAssistantService
{
    private readonly IOllamaConnectorService _ollamaConnectorService;
    private readonly ILogger<AiAssistantService> _logger;
    private readonly int tableRows = 3;

    public AiAssistantService(IOllamaConnectorService ollamaConnectorService, ILogger<AiAssistantService> logger = null!)
    {
        _ollamaConnectorService = ollamaConnectorService;
        _logger = logger;
    }

    private readonly string _databaseSchemaDescription = @"
        📘 The database schema:
        - Categories(Id PK nvarchar(50), Name nvarchar(50), CreatedAt datetime2)
        - CategoryProduct(ProductsId PK and FK nvarchar(50), CategoriesId PK and FK nvarchar(50))
        - Products(Id PK nvarchar(50), Code UNIQUE nvarchar(50), Name UNIQUE nvarchar(50), Description nvarchar(max), CreatedAt datetime2)
        - Variants(Id PK nvarchar(50), ProductId FK nvarchar(50), DiscountId FK nvarchar(50), SKU UNIQUE nvarchar(50), Price decimal, UnitsInStock int, CreatedAt datetime2)
        - Discounts(Id PK nvarchar(50), Name UNIQUE nvarchar(50), Percentage int, Description nvarchar(max), CreatedAt datetime2)
        - Coupons(Id PK nvarchar(50), Code UNIQUE nvarchar(50), Description nvarchar(max), Percentage int, UsageLimit int, DefaultDateIntervalInDays int, IsUserSpecific bit, TriggerEvent nvarchar(50), StartDate datetime2, ExpirationDate datetime2, CreatedAt datetime2)
        - UserCoupons(Id PK nvarchar(50), UserId FK nvarchar(50), CouponId nvarchar(50), Code nvarchar(50), TimesUsed int, StartDate datetime2, ExpirationDate datetime2, CreatedAt datetime2)
        - PaymentOptions(Id PK nvarchar(50), Name nvarchar(50), NameAlias nvarchar(50), Description nvarchar(max), ExtraCost decimal, CreatedAt datetime2)
        - ShippingOptions(Id PK nvarchar(50), Name nvarchar(50), Description nvarchar(max), ExtraCost decimal, ContainsDelivery bit,  CreatedAt datetime2)
        "
    ;

    public async Task<string> AskToChooseToolAsync(string userRequest)
    {
        try
        {
            string prompt = @$"You are an AI that chooses ONE TOOL CATEGORY based on the USER REQUEST.

            🎯 Your task:
            Choose One Of The Following Endpoints Category Based On the USER REQUEST:
            1) Category , choose this if the user asks about categories or product categories
            2) Product , choose this if the user asks about products or items
            3) Variant , choose this if the user asks about variants
            4) Discount, choose this if the user asks about discounts or product discounts
            5) PaymentOption, choose this if the user asks about payment options
            6) ShippingOption, choose this if the user asks about shipping options
            7) Coupon , choose this if the user asks about coupons

            ❗ Important constraints:
            1) ONLY RETURN THE NAME OF THE CATEGORY. NOTHING ELSE!
            2) IF YOU ARE NOT CERTAIN pick ONE CATEGORY of the ones you think are valid
            ";

            string result = await _ollamaConnectorService.AskOllamaAsync(userPrompt: $"User Request:'{userRequest}'", systemPrompt: prompt);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskToChooseTool method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    public async Task<string> AskToChooseEndpointAsync(string userRequest, string toolCategory)
    {
        try
        {
            string toolCategoryDescription = "";
            if (toolCategory == "Category")
            {
                toolCategoryDescription = @$"
            1) Category/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT CATEGORIES
            For example: UserInput: return all the categories. You should return 'Category/amount/{tableRows}' .NOTHING ELSE
            
            2) Category/<categoryId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <categoryId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the category with id 'randomCategoryId'. You should return 'Category/randomCategoryId' .NOTHING ELSE

            3) Category/Name/<categoryName> , RETURN THIS if the user uses the word name AND REPLACE <categoryName> WITH THE Name THE USER GIVES, fill the Name with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the category with name 'randomCategoryName'. You should return 'Category/Name/randomCategoryName' EXACTLY 'randomCategoryName' NO MATTER WHAT YOU THINK";
            }
            else if (toolCategory == "Product")
            {
                toolCategoryDescription = @$"
            1) Product/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT PRODUCTS
            For example: UserInput: return all the products. You should return 'Product/amount/{tableRows}' .NOTHING ELSE
            
            2) Product/<productId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <productId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the product with id 'randomProductId'. You should return 'Product/randomProductId' .NOTHING ELSE

            2) Product/Name/<productName> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <productName> WITH THE NAME THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the product with name 'randomProductName'. You should return 'Product/randomProductName' .NOTHING ELSE";
            }
            else if (toolCategory == "Variant")
            {
                toolCategoryDescription = @$"
            1) Variant/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT VARIANTS
            For example: UserInput: return all the variants. You should return 'Variant/amount/{tableRows}' .NOTHING ELSE
            
            2) Variant/Id/<variantId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <variantId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the variant with id 'randomVariantId'. You should return 'Variant/Id/randomVariantId' .NOTHING ELSE

            2) Variant/SKU/<variantSKU> , RETURN THIS ONLY IF THE USER USE THE WORD SKU AND REPLACE <variantSKU> WITH THE SKU THE USER GIVES, fill the SKU with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the variant with sku or SKU 'randomVariantSKU'. You should return 'Variant/SKU/randomVariantSKU' .NOTHING ELSE";
            }
            else if (toolCategory == "Discount")
            {
                toolCategoryDescription = @$"
            1) Discount/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT DISCOUNTS
            For example: UserInput: return all the discounts. You should return 'Discount/amount/{tableRows}' .NOTHING ELSE
            
            2) Discount/<discountId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <discountId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the discount with id 'randomDiscountId'. You should return 'Discount/randomDiscountId' .NOTHING ELSE

            3) Discount/Name/<discountName> , RETURN THIS ONLY IF THE USER USE THE WORD NAME AND REPLACE <discountName> WITH THE NAME THE USER GIVES, fill the NAME with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the discount with name 'randomDiscountName'. You should return 'Discount/randomDiscountName' .NOTHING ELSE";
            }
            else if (toolCategory == "PaymentOption")
            {
                toolCategoryDescription = @$"
            1) PaymentOption/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT PAYMENT OPTIONS
            For example: UserInput: return all the payment options. You should return 'PaymentOption/amount/{tableRows}' .NOTHING ELSE
            
            2) PaymentOption/<paymentOptionId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <paymentOptionId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the payment option with id 'randomPaymentOptionId'. You should return 'PaymentOption/randomPaymentOptionId' .NOTHING ELSE

            3) PaymentOption/Name/<discountName> , RETURN THIS ONLY IF THE USER USE THE WORD NAME AND REPLACE <paymentOptionName> WITH THE NAME THE USER GIVES, fill the NAME with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the payment option with name 'randomPaymentOptionName'. You should return 'PaymentOption/randomPaymentOptionName' .NOTHING ELSE
            ";
            }
            else if (toolCategory == "ShippingOption")
            {
                toolCategoryDescription = @$"
            1) ShippingOption/amount/{tableRows}, return this GENERAL QUESTIONS ABOUT SHIPPING OPTIONS
            For example: UserInput: return all the shipping options. You should return 'ShippingOption/amount/{tableRows}' .NOTHING ELSE
            
            2) ShippingOption/<shippingOptionId> , RETURN THIS ONLY IF THE USER USE THE WORD ID AND REPLACE <shippingOptionId> WITH THE ID THE USER GIVES, fill the ID with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the shipping option with id 'randomShippingOptionId'. You should return 'ShippingOption/randomShippingOptionId' .NOTHING ELSE

            3) ShippingOption/Name/<shippingOptionName> , RETURN THIS ONLY IF THE USER USE THE WORD NAME AND REPLACE <shippingOptionName> WITH THE NAME THE USER GIVES, fill the NAME with EXACTLY WHAT IS INSIDE '', or "", NOTHING ELSE!!
            For example: UserInput: return to me the shipping option with name 'randomShippingOptionName'. You should return 'ShippingOption/randomShippingOptionName' .NOTHING ELSE";
            }
            else if (toolCategory == "Coupon")
            {
                toolCategoryDescription = @"";
            }

            string prompt = @$"You are an AI that chooses ENDPOINTS based on the USER REQUEST.

            🎯 Your task:
            Choose One Of The Following Endpoints:
            {toolCategoryDescription}
            
            ❗ Important constraints:
            1) ONLY RETURN THE ENPOINTS!!!!! NOTHING ELESE
            2) NEVER EXPLAIN YOUR ANSWERS EVER!!!!!!!!!
            3) If you are uncertain pick always the most general endpoint of the ones you are considering
            ";

            string result = await _ollamaConnectorService.AskOllamaAsync(userPrompt: $"User Request:'{userRequest}'", systemPrompt: prompt);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskAIForResponseWithPlugins method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    public async Task<string> AskForJsonInterpretation(string userRequest, string jsonResultFromDataMicroservice)
    {
        try
        {
            string prompt = $@"
            You are a helpful AI that assists customers with questions about products, variants, coupons, attributes, discounts, payment options, and shipping options.

            🎯 Task:
            summarize the below JSON using the user REQUEST that you are given!!!

            🧠 Context:
            User request:
            '{userRequest}'

            📦 JSON data returned by the backend:
            {jsonResultFromDataMicroservice}
        "
        ;
            string finalResult = await _ollamaConnectorService.AskOllamaAsync(userPrompt: prompt);
            return finalResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskForInterpretationOfGeneratedQuery method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    /**************************************   **************************************/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userRequest"></param>
    /// <returns></returns>
    public async Task<string> AskToGenerateQueryAsync(string userRequest)
    {

        /*You are a helpful AI that assists customers with questions about products, variants, coupons, attributes, discounts, payment options, and shipping options.*/
        try
        {
            string prompt = @$"
        You are an AI that generates SQL Queries that are based on the database schema you will find bellow and the user request.
        You will generate **only safe, read-only SQL SELECT queries** in response to user requests. Generate **only the SQL query**. Output nothing else.
        Your queries must be written in **SQL Server (Transact-SQL)** syntax.

        ❗ Important constraints:
        - You must generate **only SELECT statements**.
        - **Never** generate any INSERT, UPDATE, DELETE, DROP, ALTER, or other modifying statements.
        - Even if the user requests something unsafe, ignore it and still generate a safe SELECT only.
        - Use only the tables and columns defined below.
        - Limit results to **TOP 5 rows**.
        - If applicable, include an **ORDER BY** or **WHERE** clause for relevance.
        - NEVER USE 'FETCH FIRST N ROWS ONLY' instead use ONLY TOP N or TOP(N), where N is obviously the number of rows
        - NEVER USE 'LIMIT N' instead use ONLY TOP N or TOP(N). NEVER DO THIS DO YOU HEAR ME!!!!!!!!!
        - NEVER CHANGE NAMES, CODES, IDs THE USER GIVES YOU INSIDE '' OR ""
        - NEVER ADD COMMENTS. EVER DO YOU HEAR ME??? IF YOU ARE UNCERTAIN OF SOMETHING JUST DONT EXPRESS IT!!!!! DO WHAT YOU ARE TOLD!

        {_databaseSchemaDescription}

        Database Schema Notes
        - There is a one-to-many relationship between the table Products and the table Variants
        - There is a one-to-one relationship between the table Variants and Discounts
        - Each variant can only have one discount
        - Variants is the tighly connected to the table Products, so if you are asked to find the price of a product then you will also think about the variant table(in this case the price exists only for the variant)
        - A variant IS AN EXTENSION OF A PRODUCT, so if you can you will try to join these 2 tables appropriately if the user asks for one or the other
        - DO NOT MIX THE PROPERTIES OF THE TABLES VARIANTS AND PRODUCTS. For example if the user asks you the price of variant you SHOULD NOT SEARCH the Products table since you know that the price exists in the variant!!!
        - There is a many-to-many relationship between the table Products and the table Categories. That is why the bridge table CategoryProduct exists.
        - Do not try to join Categories and Variants immediatelly and confuse yourself. If you need the category of a variant you WILL OBVIOUSLY have to use the many-to-many relationship of the Product and the Category.
        - Percentage column in DISCOUNT table can have values from 0-100. For example if a cell has value 30, it means 30 percent.
        - IN ORDER TO FIND THE FINAL PRICE OF A VARIANT WITH A DISCOUNT. YOU HAVE TO DO THIS TO GET THE VARIANTS FINAL PRICE: VARIANTS.PRICE - VARIANTS.PRICE * (DISCOUNTS.PERCENTAGE/100).

        🎯 Your task:
        Generate a single SQL SELECT query (T-SQL) that answers the following user request:";

            string generatedSqlQuery = await _ollamaConnectorService.AskOllamaAsync(userPrompt: $"User request: '{userRequest}'\n\n Show me the generated SQL query:", systemPrompt: prompt);
            return generatedSqlQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskAsyncToGenerateQuery method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="generatedSqlQuery"></param>
    /// <returns></returns>
    public async Task<string> AskToValidateGeneratedQueryAsync(string generatedSqlQuery)
    {
        try
        {
            string prompt = @$"You are a SQL syntax checker for SQL Server (Transact-SQL).
            Your task:
            - Accept the SQL query below.
            - Validate and fix only **syntax errors** (e.g. unmatched parentheses, invalid characters, misplaced commas).
            - **DO NOT** change the logic, joins, filters, or structure of the query.
            - If the query includes **non-SQL or invalid column definitions** (e.g., 'Id PK', 'Name UNIQUE'), convert them to valid SQL **column references only**.
            - Ensure the query is a valid **SELECT** statement only — remove any DDL like CREATE, DROP, or ALTER.
            - Flatten or remove extra line breaks and formatting, keeping the query clean and executable.
            - Dont give an explaination as to what went wrong with the previous sql query. **I want only the correct query**!
            - NO FUCKING OTHER SENTENCES OTHER THAN THE FUCKING QUERY DO YOU FUCKING UNDERSTAND. NOT A FUCKING SINGLE WORD MORE YOU PIECE OF SHIT.
            - NO CHANGING ASC TO DESC OR THE OTHER WAY AROUND. FOR ANY REASON!

            {_databaseSchemaDescription}";

            string result = await _ollamaConnectorService.AskOllamaAsync(userPrompt: $"Query: {generatedSqlQuery}", systemPrompt: prompt);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskAsyncToValidateGeneratedQuery method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userRequest"></param>
    /// <param name="generatedJsonSequences"></param>
    /// <returns></returns>
    public async Task<string> AskToCompareJsonAsync(string userRequest, List<string> generatedJsonSequences)
    {
        try
        {
            string prompt = $@"
            You are a json comparator that chooses, which json is more appropriate based on a given query and a given database schema.

            🧠 Context:
            User request:
            '{userRequest}'

            {_databaseSchemaDescription}

            ⚠️ Constraints:
            - DO NOT CHANGE THE JSON BY ANY MEANS
            - NEVER CHANGE THE JSON. FOR ANY REASON!!!!!!
            - I WANT YOU CLOWN TO SAY ADD NOTHING TO THE JSON SEQUENCE YOU CHOOSE. DO YOU HEAR THAT?? NOTHING. NOT ONE WORD, JUST RETURN IT AS IT IS

            🎯 Task:
            Return the json you think is more appropriate based on the USERREQUEST without adding ANYTHING!

            📦 Returned JSON data THAT YOU SHOULD COMPARE:"
        ;
            var chosenJson = await _ollamaConnectorService.AskOllamaAsync(userPrompt: $"1: {generatedJsonSequences[0]}\n2: {generatedJsonSequences[1]}", systemPrompt: prompt);

            return chosenJson.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskForInterpretationOfGeneratedQuery method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userRequest"></param>
    /// <param name="generatedSqlQuery"></param>
    /// <param name="jsonResultFromDataMicroservice"></param>
    /// <returns></returns>
    public async Task<string> AskForInterpretationOfGeneratedQueryAsync(string userRequest, string generatedSqlQuery, string jsonResultFromDataMicroservice)
    {
        try
        {
            string prompt = $@"
            You are a helpful AI that assists customers with questions about products, variants, coupons, attributes, discounts, payment options, and shipping options.

            🧠 Context:
            User request:
            '{userRequest}'

            {_databaseSchemaDescription}

            ⚠️ Note: The SQL query was generated under these constraints:
            - Only SELECT statements allowed
            - No modifying statements (INSERT, UPDATE, DELETE, etc.)
            - Only the above tables and columns used
            - Limit results to TOP 5 rows
            - Use WHERE/ORDER BY clauses for relevance

            The generated SQL query:
            {generatedSqlQuery}

            📦 JSON data returned by the backend:
            {jsonResultFromDataMicroservice}

            🎯 Task:
            Using all the above, provide a clear and helpful response to the user in plain language. ALSO REMEMBER TO NEVER SHOW JSON TO THE USER, THEY ARE HUMAN!
        "
        ;
            string finalResult = await _ollamaConnectorService.AskOllamaAsync(userPrompt: prompt);
            return finalResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskForInterpretationOfGeneratedQuery method");
            return "Something went wrong with the system. Please try again in a bit with a different input if possible.";
        }
    }
}
