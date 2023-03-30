CREATE TABLE [dbo].[Attractions] (
    [AttractionId] [int] NOT NULL IDENTITY,
    [AttractionName] [nvarchar](30) NOT NULL,
    [Introduction] [nvarchar](max),
    [OpenStatus] [bit] NOT NULL,
    [DistrictId] [int] NOT NULL,
    [Address] [nvarchar](100),
    [Tel] [nvarchar](10),
    [Email] [nvarchar](10),
    [Elong] [decimal](11, 8) NOT NULL,
    [Nlat] [decimal](10, 8) NOT NULL,
    [Location] [geography],
    [OfficialSite] [nvarchar](300),
    [Facebook] [nvarchar](300),
    [OpenTime] [nvarchar](150),
    [InitDate] [datetime],
    CONSTRAINT [PK_dbo.Attractions] PRIMARY KEY ([AttractionId])
)
CREATE INDEX [IX_DistrictId] ON [dbo].[Attractions]([DistrictId])
CREATE TABLE [dbo].[Districts] (
    [DistrictId] [int] NOT NULL IDENTITY,
    [DistrictName] [nvarchar](10) NOT NULL,
    [CityId] [int] NOT NULL,
    CONSTRAINT [PK_dbo.Districts] PRIMARY KEY ([DistrictId])
)
CREATE INDEX [IX_CityId] ON [dbo].[Districts]([CityId])
CREATE TABLE [dbo].[Cities] (
    [CityId] [int] NOT NULL IDENTITY,
    [CittyName] [nvarchar](10) NOT NULL,
    CONSTRAINT [PK_dbo.Cities] PRIMARY KEY ([CityId])
)
ALTER TABLE [dbo].[Attractions] ADD CONSTRAINT [FK_dbo.Attractions_dbo.Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [dbo].[Districts] ([DistrictId]) ON DELETE CASCADE
ALTER TABLE [dbo].[Districts] ADD CONSTRAINT [FK_dbo.Districts_dbo.Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [dbo].[Cities] ([CityId]) ON DELETE CASCADE
INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES (N'202303240401005_addCityDistrictAttractionTable', N'TravelMaker.Migrations.Configuration',  0x1F8B0800000000000400D55BCD6EE4B811BE07C83B083A2581D7B23D586063B477E16D8F1746C63F98F62E721BB025769B188AEC4894E3469027CB218F9457485112258AA2D4645B76CFC217357FBE2A16AB8A4556F97FFFF9EFECA7979406CF38CB096717E1E9F149186016F384B0F5455888D5773F843FFDF8C73FCC3E26E94BF09B1AF7418E83992CBF089F84D89C47511E3FE114E5C72989339EF395388E791AA184476727277F8D4E4F230C10216005C1EC73C1044971F9037ECE398BF1461488DEF204D3BC6E879E45891ADCA114E71B14E38BF03143CF98DEA2AF383BAE4687C1252508385960BA0A03C4181748009FE7BFE6782132CED68B0D3420FAB8DD6018B74234C735FFE7ED70D7A59C9CC9A544ED44051517B9E0A927E0E9875A3691397D2F09878DEC407A1F41CA622B575D4AF022BC14224371056E923B9FD34C0EB589F8B89D781468DD478D4A80E6C8BFA3605E505164F882E102A6D0A3E0A1585212FF0D6F1FF957CC2E5841A9CE24B0097D9D06687AC8F8066762FB19AF7AACDF24611075112213A201B0CEAE9679C3C487B330B80386D092E2462F34912C04CFF02F98E10C099C3C20217006DB7A93E052B23D3E06A9CADF8A2EA824585718DCA2974F98ADC513D81398D33579C1896AA859F99511B0459823B2025B581D270F0BCC7852D4FB3D481C3E9DA88F13BBDF60B600B115B922F533E71423E6CDF615C981C958ECDEA91DD24F920CE7F9C8CA4F4FDCE43E4EE711D3511A1390F89822F2F6442897B015912B1C931401CD870CBEEAD3E1873058C4486E80FF6EDC5124DE0CFC138F91AEE5BF60BECED0E669A785DEAF562486936141C4B87D4E21E06B38C0969C7F7D7342D2161FC9A8C739FD7E0A42378C882BD48A4E7E57848D7977E899ACCB2D1A30F630F88C6939207F229BEA2C3F569D5F5A470AE67C9DF1F433A7DA64BDFFCB23CAD658AA1A1F19B4E045161B6CCEA2F6BC1C3D455B9EBDCE5035ED5027A8EE577DCF4F779F3CDDE9A968EE383B1D7D9FA747990383DE07D0A09677F4772A45573A3CAAE8CA1A5C399D971B636151767C6935BFE5ADD3D1B3BE6EEFABCCAE62CDCBE4E49443999BD2205F5373D3BCE9CC0CE889EDFBD8D87EA780A3E699E660D7CBBD340FEE9199A7E6C92987D23C497B1FCD53F3DEED7A14C7BC60628200DDD3B93FA03CFF27CF92F7A72C457C90F30C7EAE08C50F600BA0726F7E1B740B0C870CF032CF3944E552C70C07D13920BB1C7C6449E0725A560BD74F5D583F5822D980ED0133B003A16963F7EC0A532C707019570F3B7394C728E98B01169478B1D59C972D5BFA234D97B1BFF4E881D1E34C5A1BA2738083898489BE87202C261B441DE463CCF50C24A5041A6A66CF15866B89740D0EF2783D1B0D35638376496C1669DA37AE94DD836968DB074EA976BFABB8E65D54702056DB6D1213699E55162E7B6D0FA6BCD4CDBAF6D7D19E5CC72A77087304CCC0997A606AA388ABA5ECC42FB6BB279C2D754492D74EDA540309BEC0C27629691DB1C50B45E3404AA8369856A17680809409B621CC6D9184395B9EABB6C955E4664CD684DF5F44F70AA60D1DBBAA999AE1781635ABE8C8B0A7678E478886D65983E91FBACB77108D1180F76532E2081D5CA1C6B752821111D85D988B244756AE0291C6F2DA0C5454A5A054AA2A1AC855CD6ED16603B19496BBAA5B824595B89A7FB7F0CFE8A4154614E796C44EC36D4309E271B4C6462F90064EAF4996CB900C2D918CE6E649DA1B66F5330376A748F65D497FEB944DAA39F2BBE7D9FA99268B73AE11AE6195A9F4ECE505C4AAF7FDC965361151948DA686E69C1629DB9D727243AC427D3B66D5E78EDACDE6E898DD1E77443D65A3E3E9EDEE687A38A6A38D8569233254C99B8EF054A33B4E999CD131CA06F7F975E64547A89B3C30AAC44A07A36A72C7A8D2273A44D5E28ED0E6487494B6D5436F3A49938EE6747ADC11DBCC888ED6B6FAE97475D13435DA969718436AAFB05D5B53AD7DA45964F8A75EC0D8F385BDD0BDEB5E9D9C6F7BE0BDD2F50E9DE00E8E7778EAFBB88B6EAEC086E7EB6E55D4AF630DDD040EB6F575BCF4CA7D2F236CFF3DB74F9B469E2328EAB1DA0052CDDFCCDE545792576E4D797FF1DF1AFBB421A1AA775F5DA2F637E43194E635B77362AB46779CF67156076A5BDD91DAC7567365BECEC07C3DED7066F4FD6E4F98DE25C81CD2506F2E43C6A567565F407657F1F56E24D5105911C29F49226F238B6D2E707A2C071C2FFE41E79460A9486AC02D6264857351A54AC2B393D333A310F0DB29CA8BF23CA1CE95797B647CA62F8E2352D63BF33BBE1559D67A38F68CB2F80965BD8AB809CADD14F69F52F4F2671D71BF92B625111394B395B2ED3DDFDDB004BF5C84FF2AA79D07377FFFD2CE3C0AEE33D0F9F3E024F8F76BEBE0ACE22ED35D7ED2D1CADE06207D113B556E5361EA456DC95B16B54D0E6E16B5ADDBA2364F3DB614B90D189DB77CCD9AB6A970CD1236BB3A7CEF8D6B262613F816259D311CFFD2B03D9CF8B4F5596FE2C06D25590E86BA47C595AB8FAC6679F847BF72A33DB6719ABA9F37D9BE5EA9CF7E7BE75737B38708A72960799B10A65BB3E27298EE55923219B059713295B9DA0B4A5E156F4DEB9B272A223964C5889E833D4481C87B56838CE67D7ED7C51F872AF6F0D2E0090B3CDEABA0C34B4B0F5EC4D1CFC10EBC225AFE556FB438A37A39016FB9E4B0D395971CCBFF0F556F8C166FD8880C27DA2DB51DDBC1CA0E1BB4BD0EC05AF53158F461C3ADCB44BE817A106BC9C268E1435F21CC94F73756E9615676F4AA78F617CC34A51CFDD74AB07AED3F93C1E3E464DD42C8FF536638EED87B33E686ADB8723B06476A8811EEDC628120C4419799202BD842E88E719E9705B9BF215A942F224B9CDCB0FB426C0A014BC6E99276C428DDD718FDB25EA5CBF3EC7E532AE9144B0036898CD2EED9CF05A149C3F7B5E5D2300021FD621D9FCBBD14324E5F6F1BA43BCE1C816AF135EEFC11A71B0A60F93D5BA067BC0F6FE02E3EE1358AB7EAD1791864F74674C53EBB22689DA134AF31DAF9F0137438495F7EFC3F0366BA9CA03F0000 , N'6.2.0-61023')
