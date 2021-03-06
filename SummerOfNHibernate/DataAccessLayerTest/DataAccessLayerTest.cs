﻿using System;
using System.Collections.Generic;
using DataAccessLayer;
using DataTransfer;
using MbUnit.Framework;
using System.Linq;
using NHamcrest.Core;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Order = DataTransfer.Order;

namespace DataAccessLayerTest
{

    [TestFixture]
    public class DataAccessLayerTests_CreateDatabase : Microdesk.Utility.UnitTest.DatabaseUnitTestBase
    {
        //[Test]
        public void CreateDatabase()
        {
            Configuration cfg = new Configuration();

            cfg.Configure();

            SchemaExport schema = new SchemaExport(cfg);


            const bool justDrop = false;
            const bool outputToConsole = true;
            const bool executeAgainstDB = true;

            schema.Drop(outputToConsole, executeAgainstDB);
            schema.Create(outputToConsole, executeAgainstDB);

            schema.Execute(outputToConsole, executeAgainstDB, justDrop);
        }

        //[Test]
        public void DeleteDatabase()
        {
            Configuration cfg = new Configuration();

            cfg.Configure();

            SchemaExport schema = new SchemaExport(cfg);


            const bool justDrop = true;
            const bool outputToConsole = true;
            const bool executeAgainstDB = true;

            schema.Drop(outputToConsole, executeAgainstDB);
            schema.Create(outputToConsole, executeAgainstDB);

            schema.Execute(outputToConsole, executeAgainstDB, justDrop);
        }
    }
    [TestFixture]
    public class DataAccessLayerTests : Microdesk.Utility.UnitTest.DatabaseUnitTestBase
    {
        private NHibernateDataProvider _provider;
        private NHibernateSessionManager _sessionManager;
        private ISession _session;

        private const string customerFirstname = "Juan";
        private const string customerLastname = "Huerta";
        private const int numberOfJuan = 11;
        private const int numberOfJuanHuerta = 6;
        private const int existingCustomerId = 2;
        private const int invalidCustomerId = -1;
        private const int customerIdWithOrders = 2;
        private const int deletableCustomerId = 90;
        private readonly Customer anyCustomer = new Customer();

        private const string notValidLastname =
            "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        private const string anyName = "anyName";
        private const int anyCustomerid = 1;
        private const string anyCustomerLastName = "anyCustomerLastName";

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            DatabaseFixtureSetUp();

            _sessionManager = new NHibernateSessionManager();
        }


        [FixtureTearDown]
        public void TestFixtureTearDown()
        {
            DatabaseFixtureTearDown();
        }

        [SetUp]
        public void Setup()
        {
            DatabaseSetUp();

            _session = _sessionManager.GetSession();

            _provider = new NHibernateDataProvider(_session);
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseTearDown();
        }



        //[Test]
        public void GetMyTestDataXMLFile()
        {
            SaveTestDatabase();
        }

        [Test]
        public void CanGetCustomerId()
        {
            ;

            var actual = _provider.GetCustomerById(existingCustomerId);

            Assert.AreEqual(existingCustomerId, actual.Id);

        }

        [Test]
        public void ReturnsNullIfItdoesNotFindACustomer()
        {
            var actual = _provider.GetCustomerById(invalidCustomerId);

            Assert.AreEqual(null, actual);
        }

        [Test]
        public void CanGetOrdersByJoiningCustomersAndOrdersAndDirtyLazyLoading()
        {
            var result = _provider.GetOrdersByJoiningCustomersAndOrderByCustomerAndDate("Lucas",
                                                                                        DateTime.Parse("1-1-1900"));
            Assert.That(result.Count, Is.GreaterThan(10));
            Assert.That(result.Select(o => o.Customer.Name.Firstname).First(), Is.EqualTo("Lucas"));
        }

        [Test]
        public void CanGetCustomersByJoiningCustomersAndOrders()
        {
            var result = _provider.GetCustomersByJoiningCustomersAndOrderByCustomerAndDate("Lucas",
                                                                                           DateTime.Parse("1-1-1975"));
            Assert.That(result.Select(s => s.Name.Firstname).First(), Is.EqualTo("Lucas"));
        }

        [Test]
        public void CanGetCustomersByFirstname()
        {
            var customers = _provider.GetCustomersByFirstname("Juan");

            foreach (var customer in customers)
            {

                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
            }

            Assert.AreEqual(customers.Count, numberOfJuan);
        }

        [Test]
        public void CanGetPreferredCustomersByFirstname()
        {
            var customers = _provider.GetPreferredCustomersByFirstname("Juan");

            foreach (var customer in customers)
            {

                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
            }

            Assert.AreEqual(customers.Count, 2);
        }

        [Test]
        public void CanGetCustomersByFirstnameWithParameters()
        {

            var customers = _provider.GetCustomerByFirstnameWithParameters(customerFirstname);

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
            }

            Assert.AreEqual(customers.Count, numberOfJuan);

        }

        [Test]
        public void CanGetCustomersByFirstnameAndLastName()
        {

            var customers = _provider.GetCustomerByFirstnameAndLastname(customerFirstname, customerLastname);

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
                Assert.AreEqual(customer.Name.Lastname, customerLastname);
            }

            Assert.AreEqual(customers.Count, numberOfJuanHuerta);
        }

        [Test]
        public void CanGetCustomersGreaterThanId()
        {
            var id = 5;
            const int minimumNumerOfOccurances = 2;
            var customers = _provider.GetCustomersWithIdGreaterThan(id);

            foreach (var customer in customers)
            {
                Assert.GreaterThan(customer.Id, id);
            }


            Assert.GreaterThan(customers.Count, minimumNumerOfOccurances);
        }

        [Test]
        public void CreteriaAPI_CanGetCustomerByFirstName()
        {
            var customers = _provider.CriteriaAPI_GetCustomerByFirstName("Juan");

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
            }

            Assert.AreEqual(customers.Count, numberOfJuan);
        }

        [Test]
        public void CreteriaAPI_CanGetCustomersByFirstnameAndLastName()
        {
            var customers = _provider.CriteriaAPI_GetCustomersByFirstNameAndLastName(customerFirstname, customerLastname);

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Firstname, customerFirstname);
                Assert.AreEqual(customer.Name.Lastname, customerLastname);
            }

            Assert.AreEqual(customers.Count, numberOfJuanHuerta);
        }

        [Test]
        public void CreteriaAPI_CanGetCustomersGreaterThanId()
        {
            var id = 5;
            const int minimumNumerOfOccurances = 2;
            var customers = _provider.CriteriaAPI_GetCustomersWithIdGreaterThan(id);

            foreach (var customer in customers)
            {
                Assert.GreaterThan(customer.Id, id);
            }

            Assert.GreaterThan(customers.Count, minimumNumerOfOccurances);
        }

        [Test]
        public void QueryByExample_CanGetCustomerByPassingACustomer()
        {
            const int minimumNumerOfOccurances = 1;

            const string lastName = "Fernandez";

            var customerSample = new Customer { Name = new Name{ Lastname = lastName } };

            var customers = _provider.QueryByExample_GetCustomerByExample(customerSample);

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Lastname, lastName);
            }


            Assert.GreaterThan(customers.Count, minimumNumerOfOccurances);
        }

        [Test]
        public void QueryByExample_CanGetCustomerByPassingACustomer_v2()
        {
            const string firstname = "Juan";
            const string lastname = "Huerta";

            var customerSample = new Customer
                                     {
                                         Name = new Name
                                                    {
                                                 Firstname = firstname,
                                                 Lastname = lastname
                                             }
                                     };

            var customers = _provider.QueryByExample_GetCustomerByExample(customerSample);

            foreach (var customer in customers)
            {
                Assert.AreEqual(customer.Name.Firstname, firstname);
                Assert.AreEqual(customer.Name.Lastname, lastname);
            }
        }

        [Test]
        public void CanGetDistinctFirstNames()
        {
            IList<string> distinctFirstnames = _provider.GetDistinctCustomerFirstNames();

            const int minimumNumberOfDistinctNames = 10;

            var numberOfDisctinctStrings = distinctFirstnames.Distinct().Count();

            var numberOfTotalStrings = distinctFirstnames.Count();


            Assert.GreaterThan(numberOfTotalStrings, minimumNumberOfDistinctNames);
            Assert.AreEqual(numberOfDisctinctStrings, numberOfTotalStrings);
        }

        [Test]
        public void CriteriaAPI_CanGetDistinctFirstNames()
        {
            IList<string> distinctFirstnames = _provider.CriteriaAPI_GetDistinctCustomerFirstNames();

            const int minimumNumberOfDistinctNames = 10;

            var numberOfDisctinctStrings = distinctFirstnames.Distinct().Count();

            var numberOfTotalStrings = distinctFirstnames.Count();

            Assert.GreaterThan(numberOfTotalStrings, minimumNumberOfDistinctNames);

            Assert.AreEqual(numberOfDisctinctStrings, numberOfTotalStrings);
        }

        [Test]
        public void CanRetrieveCustomersOrderBylastname()
        {
            IList<Customer> customers = _provider.GetCustomersOrderByLastname();

            Customer priorCustomer = null;
            foreach (var customer in customers)
            {

                if (priorCustomer != null)
                {
                    Assert.GreaterThanOrEqualTo(customer.Name.Lastname, priorCustomer.Name.Lastname);
                }

                priorCustomer = customer;
            }

        }

        [Test]
        public void CriteriaAPI_CanRetrieveCustomersOrderBylastname()
        {
            var customers = _provider.CriteriaAPI_GetCustomersOrderByLastname();

            Customer priorCustomer = null;
            foreach (var customer in customers)
            {
                if (priorCustomer != null)
                {
                    Assert.GreaterThanOrEqualTo(customer.Name.Lastname, priorCustomer.Name.Lastname);
                }

                priorCustomer = customer;
            }

        }

        [Test]
        public void CangetCountOfCustomerFirstname()
        {
            var expectedCounts = new Dictionary<string, int>
                                     {
                                         {"Juan", 11},
                                         {"Eduard", 6},
                                         {"Santiago", 2},
                                         {"Jim", 5},
                                         {"Unai", 1},
                                         {"Poi", 1},
                                         {"Hasmin", 1},
                                         {"Guillermo", 1},
                                         {"Jehanna", 1},
                                         {"Kenny", 1},
                                         {"Lucas", 1},
                                         {"Maria", 1},
                                         {"Mariasun", 1},
                                         {"Piya", 1},
                                         {"Ram", 1},
                                     };


            var firstNameCount = _provider.GetCustomersFirstnameCount();

            foreach (var nameCount in expectedCounts)
            {
                var frequency = firstNameCount.Where(s => s.Firstname == nameCount.Key).Select(p => p.Count).Single();
                var firstValue = Convert.ToInt32(frequency);

                var secondValue = Convert.ToInt32(nameCount.Value);

                Assert.AreEqual(firstValue, secondValue);
            }
        }

        [Test]
        public void CanSaveCustomer()
        {
            var newCustomerToAdd = new Customer
                                       {
                                           Name = new Name
                                           {
                                               Firstname = "Jim",
                                               Lastname = "Morrison"
                                           }
                                       };
            var customerId = _provider.AddCustomer(newCustomerToAdd);

            var customerAddedAndRetrieved = _provider.GetCustomerById(customerId);

            Assert.AreEqual(newCustomerToAdd, customerAddedAndRetrieved);
        }

        [Test]
        public void CanDeleteCustomer()
        {
            var firstCustomer = _provider.GetCustomerById(deletableCustomerId);

            _provider.DeleteCustomer(firstCustomer);

            var customerDeleted = _provider.GetCustomerById(deletableCustomerId);

            Assert.IsNull(customerDeleted);
            Assert.IsNotNull(firstCustomer);
        }

        [Test]
        public void CanGetCustomerById()
        {
            var customer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.IsNotNull(customer);

            // You can see that here you can NOT browse nor inspect the orders in the customer
            // The session has been deleted (because of the USING  statement)

            Assert.That(customer.Id, Is.EqualTo(customerIdWithOrders));

            // If lazy loading is ON, then as soon as the session is deleted (because of "using session"
            // then we wont be able to access the orders.


            /* No more exception thrown, because the session is not closed, so we can lazy load as much as we want
             * Assert.Throws<LazyInitializationException>(delegate
                                                           {
                                                               var numberOfOrders = customer.Orders.Count;
                                                           }); 
             */

            // The session is not destroyed, as such we can retrieve the orders.
            Assert.That(customer.Orders.Count, Is.GreaterThan(0));
        }

        [Test]
        public void CanGetCustomerByIdNoUsingUsingSession()
        {
            var customer = _provider.GetCustomerByIdNoUsingsession(customerIdWithOrders);

            // You can see that here you can browse and inspect the orders in the customer
            // The session is still active
            Assert.GreaterThan(customer.Orders.Count, 0);
            Assert.IsNotNull(customer);
            Assert.That(customer.Id, Is.EqualTo(customerIdWithOrders));
        }

        [Test]
        public void CanGetCustomerAndOrdersByCustomerIdEvenIsNoLazyLoadingAndSessionIsDeleted()
        {
            var customer = _provider.GetCustomerByIdNoUsingsession(customerIdWithOrders);
            Assert.GreaterThan(customer.Orders.Count, 0);
            Assert.IsNotNull(customer);
            Assert.That(customer.Id, Is.EqualTo(customerIdWithOrders));
        }

        [Test]
        public void CanUpdateCustomerFirstname()
        {
            const int specificCustomerId = 8;

            var firstCustomer = _provider.GetCustomerById(specificCustomerId);

            var newNameToUpdate = firstCustomer.Name.Firstname + "_newName";

            _provider.UpdateCustomerFirstname(specificCustomerId, newNameToUpdate);

            var updatedName = _provider.GetCustomerById(specificCustomerId).Name.Firstname;

            Assert.AreEqual(updatedName, newNameToUpdate);
        }

        [Test]
        public void CanUpdateCustomerLastname()
        {
            int specificCustomerId = 6;
            String updatedLastame;
            String newLastnameToUpdate;

            using (_session)
            {
                var firstCustomer = _provider.GetCustomerById(specificCustomerId);
                newLastnameToUpdate = firstCustomer.Name.Lastname + "_newName";
            }

            ResetSessionForProvider();
            using (_session)
            {

                _provider.UpdateCustomerLastname(specificCustomerId, newLastnameToUpdate);
            }

            ResetSessionForProvider();
            using (_session)
            {

                updatedLastame = _provider.GetCustomerById(specificCustomerId).Name.Lastname;
            }

            Assert.AreEqual(updatedLastame, newLastnameToUpdate);
        }

        [Test]
        public void CanUpdateCustomer()
        {
            const int specificCustomerId = 8;
            var firstCustomer = _provider.GetCustomerById(specificCustomerId);
            var newName = firstCustomer.Name.Firstname + "_newName";
            var newLastname = firstCustomer.Name.Lastname + "_newLastname";

            firstCustomer.Name.Firstname = newName;
            firstCustomer.Name.Lastname = newLastname;

            _provider.UpdateCustomer(firstCustomer);

            var updateCustomer = _provider.GetCustomerById(specificCustomerId);

            Assert.AreEqual(updateCustomer.Name.Firstname, newName);
            Assert.AreEqual(updateCustomer.Name.Lastname, newLastname);
        }

        [Test]
        public void DeleteCustomerCanThrowExceptionOnFail()
        {
            var undeletableCustomer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.Throws<GenericADOException>(
                delegate
                    {
                        _provider.DeleteCustomer(undeletableCustomer);
                    });

            // Sames as above with lambda
            Assert.Throws<GenericADOException>(
                () => _provider.DeleteCustomer(undeletableCustomer));


        }

        [Test]
        public void DeleteCustomerWithTransactionCanThrowExceptionAndRollBack()
        {
            Customer undeletableCustomer;
            Customer deletableCustomer;
            Customer deletableCustomerRecoverd;

            using (_session)
            {
                undeletableCustomer = _provider.GetCustomerById(customerIdWithOrders);
            }

            ResetSessionForProvider();
            using (_session)
            {
                deletableCustomer = _provider.GetCustomerById(deletableCustomerId);
            }


            ResetSessionForProvider();
            using (_session)
            {


                Assert.Throws<GenericADOException>(delegate
                                                       {
                                                           _provider.DeleteCustomerWithTransactionCanRollBack(
                                                               undeletableCustomer,
                                                               deletableCustomer);
                                                       });
            }

            ResetSessionForProvider();
            using (_session)
            {
                deletableCustomerRecoverd = _provider.GetCustomerById(deletableCustomerId);
            }

            Assert.That(deletableCustomerRecoverd.Id, Is.EqualTo(deletableCustomerId));
        }

        [Test]
        public void DeleteCustomerWithTransactionRollbackWhenExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act

            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.DeleteCustomerWithTransaction(anyCustomer);
                                         });


            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());


        }

        [Test]
        public void GetCustomersByFirstname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetCustomersByFirstname("anyFirstName");
                                         });


            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        private static NHibernateDataProvider GetMockProvider(MockTransaction mockTransaction)
        {
            var mockSession = new MockSession(mockTransaction);
            var mockSessionFactory = new MockSessionFactory(mockSession);

            return new NHibernateDataProvider(mockSession);
        }

        [Test]
        public void GetCustomerByFirstnameWithParameters_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetCustomerByFirstnameWithParameters("anyFirstName");
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }


        [Test]
        public void UpdateCustomer_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.UpdateCustomer(anyCustomer);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }


        [Test]
        public void AddCustomer_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.AddCustomer(anyCustomer);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }


        [Test]
        public void UpdateCustomerLastname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.UpdateCustomerLastname(anyCustomerid, anyCustomerLastName);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void DeleteCustomerWithTransactionCanRollBack_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.DeleteCustomerWithTransactionCanRollBack(anyCustomer,
                                                                                                   anyCustomer);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void UpdateCustomerFirstname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.UpdateCustomerFirstname(anyCustomerid, anyCustomerLastName);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }


        [Test]
        public void DeleteCustomerWithTransaction_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.DeleteCustomerWithTransaction(anyCustomer);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void GetCustomerByFirstnameAndLastname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetCustomerByFirstnameAndLastname("anyname", "anylastname");
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void GetCustomersWithIdGreaterThan_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             const int anyId = 1;
                                             mockProvider.GetCustomersWithIdGreaterThan(anyId);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void CriteriaAPI_GetCustomerByFirstName_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.CriteriaAPI_GetCustomerByFirstName(anyName);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void CriteriaAPI_GetCustomersByFirstNameAndLastName_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.CriteriaAPI_GetCustomersByFirstNameAndLastName(anyName,
                                                                                                         anyCustomerLastName);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void CriteriaAPI_GetCustomersWithIdGreaterThan_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             const int anyId = 1;
                                             mockProvider.CriteriaAPI_GetCustomersWithIdGreaterThan(anyId);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void QueryByExample_GetCustomerByExample_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.QueryByExample_GetCustomerByExample(anyCustomer);
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void GetDistinctCustomerFirstNames_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetDistinctCustomerFirstNames();
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void CriteriaAPI_GetDistinctCustomerFirstNames_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.CriteriaAPI_GetDistinctCustomerFirstNames();
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void GetCustomersOrderByLastname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetCustomersOrderByLastname();
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void CriteriaAPI_GetCustomersOrderByLastname_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.CriteriaAPI_GetCustomersOrderByLastname();
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void GetCustomersFirstnameCount_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
                                         {
                                             mockProvider.GetCustomersFirstnameCount();
                                         });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        } 

        [Test]
        public void GetCustomerAndOrdersByCustomerId_RollsBack_WhenHibernateExceptionIsThrown()
        {
            // Arrange
            var mockTransaction = new MockTransaction();
            var mockProvider = GetMockProvider(mockTransaction);

            // Act
            Assert.Throws<Exception>(delegate
            {
                mockProvider.GetCustomerAndOrdersByCustomerId(anyCustomerid);
            });

            // Assert
            Assert.That(mockTransaction.WasRolledBack, Is.True());
        }

        [Test]
        public void UpdateCustomerWithExtraLongNameWillThrowExceptionAnfFail()
        {
            var customer = _provider.GetCustomerById(existingCustomerId);
            customer.Name.Firstname =
                "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

            Assert.Throws<HibernateException>(delegate
                                                  {
                                                      _provider.UpdateCustomer(customer);
                                                  });
        }

        [Test]
        public void SaveOrUpdateCustomersWorks()
        {
            const string name = "Juan";
            var juans = _provider.GetCustomersByFirstname(name);
            const string newLastName = "deTomates";

            foreach (var juan in juans)
            {
                juan.Name.Lastname = newLastName;
            }

            var newJuan_1 = new Customer { Name = new Name{ Firstname = name, Lastname = newLastName } };
            var newJuan_2 = new Customer { Name = new Name{ Firstname = name, Lastname = newLastName } };

            juans.Add(newJuan_1);
            juans.Add(newJuan_2);

            var numberOfJuansAtTheBeginning = juans.Count;
            _provider.SaveOrUpdateCustomers(juans);
            var numberOfJuansAfterUpdatingAndSaving =
                _provider.GetCustomerByFirstnameAndLastname(name, newLastName).Count;

            Assert.AreEqual(numberOfJuansAtTheBeginning, numberOfJuansAfterUpdatingAndSaving);
        }

        [Test]
        public void SaveOrUpdateCustomersCanAddCustomersWithSameNameAndLastName()
        {
            var sameNameLastNameCustomers = new List<Customer>();
            const string name = "Firstname";
            const string lastname = "Lastname";
            const int numberOfCustomersAdded = 10;
            for (var i = 0; i < numberOfCustomersAdded; i++)
            {
                sameNameLastNameCustomers.Add(
                    new Customer { Name = new Name{ Firstname = name, Lastname = lastname } }
                    );
            }

            _provider.SaveOrUpdateCustomers(sameNameLastNameCustomers);

            var numberOfCustomersInserteInDB = _provider.GetCustomerByFirstnameAndLastname(name, lastname).Count;

            Assert.AreEqual(numberOfCustomersInserteInDB, numberOfCustomersAdded);
        }

        [Test]
        public void SaveOrUpdateCustomersWorksRollsBackIfExcepionIsThrown()
        {
            const string name = "Juan";
            IList<Customer> juans;
            using (_session)
            {
                juans = _provider.GetCustomersByFirstname(name);
            }
            ResetSessionForProvider();

            const string newLastName = "deTomates";

            foreach (var juan in juans)
            {
                juan.Name.Lastname = newLastName;
            }

            var newJuan_1 = new Customer { Name = new Name{ Firstname = name, Lastname = newLastName } };
            var newJuan_2 = new Customer { Name = new Name{ Firstname = name, Lastname = newLastName } };
            var newJuan_Invalid = new Customer
                                      {
                                          Name = new Name
                                          {
                                              Firstname = name,
                                              Lastname = notValidLastname
                                          }
                                      };
            juans.Add(newJuan_1);
            juans.Add(newJuan_2);
            juans.Add(newJuan_Invalid);

            var HibernateException_was_thrown = false;

            using (_session)
            {
                try
                {
                    _provider.SaveOrUpdateCustomers(juans);
                }
                catch (HibernateException)
                {
                    HibernateException_was_thrown = true;
                }
            }
            ResetSessionForProvider();

            var numberOfJuansAfterUpdatingAndSaving =
                _provider.GetCustomerByFirstnameAndLastname(name, newLastName).Count;

            const int zeroAsAResultOfNotBeingExecuted = 0;

            Assert.That(HibernateException_was_thrown, Is.True());
            Assert.That(numberOfJuansAfterUpdatingAndSaving, Is.EqualTo(zeroAsAResultOfNotBeingExecuted));
        }

        [Test]
        // An specific exception is thrown on concurrency! In this case, just grab the
        // the exception and act as you wish.
        public void CanThrowExceptionONConcurrencyViolationUpdate_OptimisticConcurrency()
        {
            Customer customer_id_1;
            using (_session)
            {
                customer_id_1 = _provider.GetCustomerById(existingCustomerId);
                customer_id_1.Name.Firstname = "First Change";
            }


            Customer same_customer_id_1;
            ResetSessionForProvider();
            using (_session)
            {
                same_customer_id_1 = _provider.GetCustomerById(existingCustomerId);
                same_customer_id_1.Name.Firstname = "Second Change";
            }

            ResetSessionForProvider();
            using (_session)
            {
                _provider.UpdateCustomer(customer_id_1);
            }

            ResetSessionForProvider();
            using (_session)
            {
                Assert.Throws<StaleObjectStateException>(delegate
                                                             {
                                                                 _provider.UpdateCustomer(same_customer_id_1);
                                                             });
            }

        }

        [Test]
        // An specific exception is thrown on concurrency! In this case, just grab the
        // the exception and act as you wish.
        public void CanThhrowExceptionONConcurrencyViolationDelete_OptimisticConcurrency()
        {
            Customer customer_id_1;
            Customer same_customer_id_1;

            using (_session)
            {
                customer_id_1 = _provider.GetCustomerById(deletableCustomerId);
            }
            customer_id_1.Name.Firstname = "First Change";

            ResetSessionForProvider();

            using (_session)
            {
                same_customer_id_1 = _provider.GetCustomerById(deletableCustomerId);
            }
            same_customer_id_1.Name.Firstname = "Second Change";

            ResetSessionForProvider();

            using (_session)
            {
                _provider.DeleteCustomer(customer_id_1);
            }

            ResetSessionForProvider();

            using (_session)
            {
                Assert.Throws<StaleObjectStateException>(delegate
                                                             {

                                                                 _provider.DeleteCustomer(same_customer_id_1);

                                                             });
            }

        }

        private void ResetSessionForProvider()
        {

            if (_session.IsOpen)
            {
                _session.Close();
            }

            _session = _sessionManager.GetSession();
            _provider.Session = _session;


        }

        [Test]
        public void CanGetDistinctCustomersWithOrdersSince()
        {
            var orderDateSince = DateTime.Parse("1/1/1950");
            var customersOrderSince = _provider.GetDistinctCustomersWithOrdersSince(orderDateSince);
            foreach (var customer in customersOrderSince)
            {
                foreach (var order in customer.Orders)
                {
                    Assert.That(order.OrderDate, Is.GreaterThan(orderDateSince));
                }
            }

            foreach (var customer in customersOrderSince)
            {
                // Assering we have unique (single) customers
                Assert.AreEqual(1, customersOrderSince.Count(c => c == customer));
            }
        }

        [Test]
        public void CriteriaAPI_CanGetDistinctCustomersWithOrdersSince()
        {
            var orderDateSince = DateTime.Parse("1/1/1950");
            var customersOrderSince = _provider.CriteriaAPI_GetDistinctCustomersWithOrdersSince(orderDateSince);
            foreach (var customer in customersOrderSince)
            {
                foreach (var order in customer.Orders)
                {
                    Assert.That(order.OrderDate, Is.GreaterThan(orderDateSince));
                }
            }

            foreach (var customer in customersOrderSince)
            {
                // Asserting we have unique (single) customers
                Assert.AreEqual(1, customersOrderSince.Count(c => c == customer));
            }
        }

        [Test]
        public void CriteriaAPI_GetDistinctCustomersWithOrdersSinceWithProjects()
        {
            var orderDateSince = DateTime.Parse("1/1/1950");
            var customersOrderSince =
                _provider.CriteriaAPI_GetDistinctCustomersWithOrdersSinceWithProjects(orderDateSince);
            foreach (var customer in customersOrderSince)
            {
                foreach (var order in customer.Orders)
                {
                    Assert.That(order.OrderDate, Is.GreaterThan(orderDateSince));
                }
            }

            foreach (var customer in customersOrderSince)
            {
                // Assering we have unique (single) customers
                Assert.AreEqual(1, customersOrderSince.Count(c => c == customer));
            }
        }

        [Test]
        public void CanGetCustomersWithOrdersThatContainAnSpecificProductID()
        {
            const int productId = 3;
            var customers = _provider.GetCustomersWithORdersHavingProduct(productId).Distinct();
            foreach (var customer in customers)
            {
                var atLeastOneOrderContainsProductId =
                    customer.Orders.SelectMany(o => o.Products).Select(p => p.Id).Contains(productId);
                Assert.That(atLeastOneOrderContainsProductId, Is.True());

            }
        }

        [Test]
        public void CanAddCustomer()
        {
            Customer customer = new Customer() { Name = new Name() { Firstname = "Steve", Lastname = "Bohlen" } };

            int newIdentity = _provider.AddCustomerV2(customer);

            Customer testCustomer = _provider.GetCustomerById(newIdentity);

            Assert.IsNotNull(testCustomer);

        }


        [Test]
        public void CanUpdateCustomer_V2()
        {
            Customer customer = _provider.GetCustomerById(existingCustomerId);

            string originalFirstname = customer.Name.Firstname;

            string newFirstname = string.Concat(originalFirstname, "SUFFIX");

            customer.Name.Firstname = newFirstname;

            _provider.UpdateCustomer(customer);

            Customer testCustomer = _provider.GetCustomerById(existingCustomerId);

            Assert.AreEqual(newFirstname, testCustomer.Name.Firstname);

        }

        [Test]
        public void CanGetCustomerWithToysView()
        {
            var orderDate = DateTime.Parse("5/5/1950");
            var customers = _provider.GetCustomersWithToys(orderDate);
            foreach (var customersWithToys in customers)
            {
                Assert.That(customersWithToys.OrderDate, Is.GreaterThan(orderDate));
                var customer = _provider.GetCustomerById(customersWithToys.CustomerId);
                var customerHasSomeOrderWithToys =
                    customer.Orders.SelectMany(o => o.Products).Select(p => p.Name).Contains("toys");
                Assert.That(customerHasSomeOrderWithToys, Is.True());
            }
        }

        [Test]
        public void NameComponentCanReturnFullname()
        {
            const string customerFullname = "Juan";
            const string customerLastname = "Huerta";
            var expectedFullNameFormat = String.Format("{0} {1}", customerFirstname, customerLastname);

            var customers = _provider.GetCustomerByFirstnameAndLastname(customerFullname, customerLastname);
            foreach (var customer in customers)
            {
                Assert.That(customer.Name.Fullname, Is.EqualTo(expectedFullNameFormat));
            }
        }

        [Test]
        public void CanEvictCustomerFromSession()
        {
            var customer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.IsTrue(_session.Contains(customer));
            
            _session.Evict(customer);

            Assert.IsFalse(_session.Contains(customer));

        }

        [Test]
        public void CustomerIsNotAssociatedWithNewSession()
        {
            var customer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.IsTrue(_session.Contains(customer));

            ResetSessionForProvider();

            Assert.IsFalse(_session.Contains(customer));

        }

        [Test]
        public void CustomerCanBeReassociatedWithNewSession()
        {
            var customer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.IsTrue(_session.Contains(customer));

            ResetSessionForProvider();

            Assert.IsFalse(_session.Contains(customer));

            _provider.UpdateCustomer(customer);

            Assert.IsTrue(_session.Contains(customer));
        }

        [Test]
        public void CustomerCanBeReassociatedWithNewSessionAndSetNonDirtyIfNoModifyAvodingUpdate()
        {
            var customer = _provider.GetCustomerById(customerIdWithOrders);

            Assert.IsTrue(_session.Contains(customer));

            ResetSessionForProvider();

            Assert.IsFalse(_session.Contains(customer));

            _session.Lock(customer,NHibernate.LockMode.None);

            _provider.UpdateCustomer(customer);

            Assert.IsTrue(_session.Contains(customer));
        }

        [Test]
        public void CanGetCustomerByFirstnameUsingDetachedCriteria()
        {
            var JuanHuertaCustomers =
                DetachedCriteria.For<Customer>()
                .Add(Expression.Eq("Name.Firstname", "Juan"))
                .Add(Expression.Eq("Name.Lastname", "Huerta"));


            var customers = JuanHuertaCustomers.GetExecutableCriteria(_session).List<Customer>();
            
            var numberOfJuanHuertaCustomers = customers.Where(x => (x.Name.Firstname == "Juan" && x.Name.Lastname == "Huerta")).Count();

            Assert.That(numberOfJuanHuertaCustomers,Is.EqualTo(customers.Count));
        }

        [Test]
        public void CanGetCustomerByFirstnameUsingDetachedCriteriaMethod()
        {
            var JuanHuertaCustomers =
                DetachedCriteria.For<Customer>()
                .Add(Expression.Eq("Name.Firstname", "Juan"))
                .Add(Expression.Eq("Name.Lastname", "Huerta"));


            var customers = _provider.GetCustomerByArbitraryCriteria(JuanHuertaCustomers);

            var numberOfJuanHuertaCustomers = customers.Where(x => (x.Name.Firstname == "Juan" && x.Name.Lastname == "Huerta")).Count();

            Assert.That(numberOfJuanHuertaCustomers, Is.EqualTo(customers.Count));
        }

        [Test]
        public void CanGetCustomerByFirstNameUsingGenericDataProvider()
        {
            var JuanHuertaCustomers =
                DetachedCriteria.For<Customer>()
                    .Add(Expression.Eq("Name.Firstname", "Juan"))
                    .Add(Expression.Eq("Name.Lastname", "Huerta"));

            var _genericProvider = new GenericDataProvider<Customer>(_session);


            var customersBySpecificProvider = _provider.GetCustomerByArbitraryCriteria(JuanHuertaCustomers);
            var customersByGenericProvider = _genericProvider.Find(JuanHuertaCustomers);

            var numberOfcustomersBySpecificProviderSerialized = customersBySpecificProvider.Count;
            var numberOfcustomersByGenericProviderSerialized = customersByGenericProvider.Count;
            
            Assert.That(numberOfcustomersByGenericProviderSerialized,Is.Not(0));
            Assert.That(numberOfcustomersByGenericProviderSerialized, Is.EqualTo(numberOfcustomersBySpecificProviderSerialized));
        }

        [Test]
        public void CanGetCustomerByFirstNameUsingSuperCustomerClass()
        {
            var name = new Name()
                           {
                               Firstname = "Juan",
                               Lastname = "Huerta"
                           };

            var superCustomerClass = new SuperCustomerClassDataProvider(_session);

            var customersBySuperCustomerClass = superCustomerClass.GetCustomerByName(name);
            var numberOfJuanFirstnames = customersBySuperCustomerClass.Where(x => x.Name.Firstname == "Juan").Count();
            var numberOfJuanLastnames = customersBySuperCustomerClass.Where(x => x.Name.Lastname == "Huerta").Count();

            Assert.That(customersBySuperCustomerClass.Count, Is.GreaterThan(0));
            Assert.That(numberOfJuanFirstnames, Is.EqualTo(numberOfJuanLastnames));
            Assert.That(numberOfJuanFirstnames, Is.EqualTo(customersBySuperCustomerClass.Count));
        }

        [Test]
        public void CanUseGenericDataProviderForCustomer()
        {
            var genericCustomerProvider = new GenericDataProvider<Customer>(_session);
            var specificCustomerProvider = new CustomerDataProvider(_session);

            int customerId = 9;
            var customerListByGeneric = genericCustomerProvider.GetById(customerId);
            var customerListBySpecific = specificCustomerProvider.GetCustomerById(customerId);

            Assert.That(customerListBySpecific, Is.EqualTo(customerListByGeneric));
        }

        [Test]
        public void CanUseSuperOrderClassToGetOrder()
        {
            var superOrderClass = new SuperOrderClassDataProvider(_session);
            var orderId = 20;

            var order = superOrderClass.GetOrderById(orderId);
            Assert.That(order.Id,Is.EqualTo(orderId));
        }

        [Test]
        public void CanUseGenericDataProviderForProduct()
        {
            var genericProductProvider = new GenericDataProvider<Product>(_session);

            var specificProductProvider = new ProductDataProvider(_session);

            int productId = 20;
            var productListByGeneric = genericProductProvider.GetById(productId);
            var productListBySpecific = specificProductProvider.GetProductById(productId);

            Assert.That(productListBySpecific, Is.EqualTo(productListByGeneric));
        }

        [Test]
        public void CanUseGenericDataProviderForOrder()
        {
            var genericOrderProvider = new GenericDataProvider<Order>(_session);
            var specificOrderProvider = new OrderDataProvider(_session);

            int orderId = 20;
            var orderListByGeneric = genericOrderProvider.GetById(orderId);
            var orderListBySpecific = specificOrderProvider.GetOrderById(orderId);

            Assert.That(orderListBySpecific, Is.EqualTo(orderListByGeneric));
        }

    }
}