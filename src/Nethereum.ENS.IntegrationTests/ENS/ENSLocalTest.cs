using Nethereum.ENS.ENSRegistry.ContractDefinition;
using Nethereum.ENS.FIFSRegistrar.ContractDefinition;
using Nethereum.ENS.PublicResolver.ContractDefinition;
using Nethereum.Geth;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.XUnitEthereumClients;
using Xunit;

namespace Nethereum.ENS.IntegrationTests.ENS
{
    public class ENSMainNetTest
    {
        public async void ShouldBeAbleToRegisterExample()
        {
            var durationInDays = 365;
            var ourName = "lllalalalal"; //enter owner name
            var tls = "eth";
            var owner = "0x111F530216fBB0377B4bDd4d303a465a1090d09d";
            var secret = "Today is gonna be the day That theyre gonna throw it back to you"; //make your own


            var web3 = new Web3.Web3(new Account(""), "https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ethTLSService = new EthTLSService(web3);
            await ethTLSService.InitialiseAsync();

            var price = await ethTLSService.CalculateRentPriceInEtherAsync(ourName, durationInDays);
            Assert.True(price > 0);

            var commitment = await ethTLSService.CalculateCommitmentAsync(ourName, owner, secret);
            var commitTransactionReceipt = await ethTLSService.CommitRequestAndWaitForReceiptAsync(commitment);
            var txnHash = await ethTLSService.RegisterRequestAsync(ourName, owner, durationInDays, secret, price);
        }

        public async void ShouldBeAbleToSetTextExample()
        {
            var web3 = new Web3.Web3(new Account(""), "https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ensService = new ENSService(web3);
            var txn = await ensService.SetTextRequestAsync("nethereum.eth", TextDataKey.url, "https://nethereum.com");
        }

        [Fact]
        public async void ShouldBeAbleToResolveText()
        {
            var web3 = new Web3.Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ensService = new ENSService(web3);
            var url = await ensService.ResolveTextAsync("nethereum.eth", TextDataKey.url);
            Assert.Equal("https://nethereum.com", url);
        }

        [Fact]
        public async void ShouldBeAbleToCalculateRentPriceAndCommitment()
        {
            var durationInDays = 365;
            var ourName = "supersillynameformonkeys";
            var tls = "eth";
            var owner = "0x12890d2cce102216644c59dae5baed380d84830c";
            var secret = "animals in the forest";


            var web3 = new Web3.Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ethTLSService = new EthTLSService(web3);
            await ethTLSService.InitialiseAsync();

            var price = await ethTLSService.CalculateRentPriceInEtherAsync(ourName, durationInDays);
            Assert.True(price > 0);

            var commitment = await ethTLSService.CalculateCommitmentAsync(ourName, owner, secret);
            Assert.Equal("0x546d078db03381f4a33a33600cf1b91e00815b572c944f4a19624c8d9aaa9c14", commitment.ToHex(true));

        }


        [Fact]
        public async void ShouldResolveOwnerFromMainnet()
        {
            var web3 = new Web3.Web3("https://mainnet.infura.io");
            var fullNameNode = new EnsUtil().GetNameHash("juanblanco.eth");
            var ensRegistryService = new ENSRegistryService(web3, "0x00000000000C2E074eC69A0dFb2997BA6C7d2e1e");
            var owner = await ensRegistryService.OwnerQueryAsync(fullNameNode.HexToByteArray());
            Assert.Equal("0x24ecd23096fcf03a15ee8a6fe63f24345cc4ba46", owner);
        }

        [Fact]
        public async void ShouldFindEthControllerFromMainnet()
        {
            var web3 = new Web3.Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ethTLSService = new EthTLSService(web3);
            await ethTLSService.InitialiseAsync();
            var controllerAddress = ethTLSService.TLSControllerAddress;
            Assert.True("0x283Af0B28c62C092C9727F1Ee09c02CA627EB7F5".IsTheSameAddress(controllerAddress));
            
        }

        [Fact]
        public async void ShouldResolveAddressFromMainnet()
        {
            var web3 = new Web3.Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ensService = new ENSService(web3);
            var theAddress = await ensService.ResolveAddressAsync("nick.eth");     
            var expectedAddress = "0xb8c2C29ee19D8307cb7255e1Cd9CbDE883A267d5";
            Assert.True(expectedAddress.IsTheSameAddress(theAddress));   
        }

        //[Fact]
        public async void ShouldReverseResolveAddressFromMainnet()
        {
            var web3 = new Web3.Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var ensService = new ENSService(web3);
            var name = await ensService.ReverseResolveAsync("0xb8c2C29ee19D8307cb7255e1Cd9CbDE883A267d5");
            var expectedName = "nick.eth";
            Assert.Equal(expectedName, name);
        }

    }

    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class ENSLocalTest
    {

        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        public ENSLocalTest(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
        }


        [Fact]
        public async void ShouldCreateEnsRegistarResolverAndRegiterandResolveANewAddress()
        {
            //Ignoring parity due to https://github.com/paritytech/parity-ethereum/issues/8675
            if (_ethereumClientIntegrationFixture.Geth)
            {
                //The address we want to resolve when using "test.eth"
                var addressToResolve = "0x12890d2cce102216644c59dae5baed380d84830c";


                var addressFrom = "0x12890d2cce102216644c59dae5baed380d84830c";

                var web3 = _ethereumClientIntegrationFixture.GetWeb3();


                //deploy ENS contract
                var ensDeploymentReceipt =
                    await ENSRegistryService.DeployContractAndWaitForReceiptAsync(web3, new ENSRegistryDeployment());

                var ensUtil = new EnsUtil();
                var ethNode = ensUtil.GetNameHash("eth");

                //create a new First in First service registrar for "eth"
                var fifsDeploymentReceipt = await FIFSRegistrarService.DeployContractAndWaitForReceiptAsync(web3,
                    new FIFSRegistrarDeployment()
                    {
                        EnsAddr = ensDeploymentReceipt.ContractAddress,
                        Node = ethNode.HexToByteArray()
                    });


                var publicResolverDeploymentReceipt = await PublicResolverService.DeployContractAndWaitForReceiptAsync(
                    web3,
                    new PublicResolverDeployment() {Ens = ensDeploymentReceipt.ContractAddress}
                );


                var ensRegistryService = new ENSRegistryService(web3, ensDeploymentReceipt.ContractAddress);

                //set ownership of "eth" to the fifs service
                //we are owners of "", so a subnode label "eth" will now be owned by the FIFS registar, which will allow to also to set ownership in Ens of further subnodes of Eth.
                var ethLabel = ensUtil.GetLabelHash("eth");

                var receipt = await ensRegistryService.SetSubnodeOwnerRequestAndWaitForReceiptAsync(
                    ensUtil.GetNameHash("").HexToByteArray(),
                    ethLabel.HexToByteArray(),
                    fifsDeploymentReceipt.ContractAddress
                );


                //Now the owner of Eth is the FIFS
                var ownerOfEth =
                    await ensRegistryService.OwnerQueryAsync(ethNode.HexToByteArray());
                Assert.Equal(fifsDeploymentReceipt.ContractAddress.ToLower(), ownerOfEth.ToLower());
                /**** setup done **/

                //registration of "myname"

                //create a service for the registrar
                var fifsService = new FIFSRegistrarService(web3, fifsDeploymentReceipt.ContractAddress);

                //create a label
                var testLabel = ensUtil.GetLabelHash("myname");
                //submit the registration using the label bytes, and set ourselves as the owner
                await fifsService.RegisterRequestAndWaitForReceiptAsync(new RegisterFunction()
                {
                    Owner = addressFrom,
                    Subnode = testLabel.HexToByteArray()
                });

                

                //now using the the full name
                var fullNameNode = ensUtil.GetNameHash("myname.eth");

                var ownerOfMyName =
                    await ensRegistryService.OwnerQueryAsync(fullNameNode.HexToByteArray());
                //set the resolver (the public one)
                await ensRegistryService.SetResolverRequestAndWaitForReceiptAsync(
                    new SetResolverFunction()
                    {

                        Resolver = publicResolverDeploymentReceipt.ContractAddress,
                        Node = fullNameNode.HexToByteArray()
                    });


                var publicResolverService =
                    new PublicResolverService(web3, publicResolverDeploymentReceipt.ContractAddress);
                // set the address in the resolver which we want to resolve, ownership is validated using ENS in the background

                //Fails here
                await publicResolverService.SetAddrRequestAndWaitForReceiptAsync(fullNameNode.HexToByteArray(),
                    addressToResolve
                );


                //Now as "end user" we can start resolving... 

                //get the resolver address from ENS
                var resolverAddress = await ensRegistryService.ResolverQueryAsync(
                    fullNameNode.HexToByteArray());

                //using the resolver address we can create our service (should be an abstract / interface based on abi as we can have many)
                var resolverService = new PublicResolverService(web3, resolverAddress);

                //and get the address from the resolver
                var theAddress =
                    await resolverService.AddrQueryAsync(fullNameNode.HexToByteArray());

                Assert.Equal(addressToResolve.ToLower(), theAddress.ToLower());
            }

        }
    }
}