#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SendMessageCommandTests : BaseUnitTest
    { 


        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendBadRecipientInfo()
        {

            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";

            TestHelper.SetPrincipal(sender);
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Recipient = "Do you like chocolate", Message = id, Subject = "All That Matters" }, false, false);
            var response = await cmd.Execute();

            Assert.AreEqual(Status.Invalid, response.Status, response.Message);
            Assert.AreEqual(null, response.Response, "Expecting null return payload");

            using (var db = new voatEntities())
            {
                var count = (from x in db.Messages
                             where
                               x.Content == id
                             select x).Count();
                Assert.AreEqual(0, count, "Expecting no messages to make it through");
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendBadRecipientInfo_CheckExists()
        {

            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";

            TestHelper.SetPrincipal(sender);
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Recipient = "Do you like chocolate", Message = id, Subject = "All That Matters" }, false, true);
            var response = await cmd.Execute();

            Assert.IsFalse(response.Success, "Expecting failure");
            Assert.AreNotEqual("Comment points too low to send messages. Need at least 10 CCP.", response.Message);
            
            using (var db = new voatEntities())
            {
                var count = (from x in db.Messages
                              where
                                x.Content == id
                              select x).Count();
                Assert.AreEqual(0, count, "Expecting no messages to make it through");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageDeniedCCP()
        {
            var id = Guid.NewGuid().ToString();
            var sender = "unit";
            var recipient = "anon";

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsFalse(response.Success, "Expecting not success response");

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessage() {

            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";
            var recipient = "anon";

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Message);

            using (var db = new voatEntities())
            {
                var record = (from x in db.Messages
                              where 
                                x.Recipient == recipient 
                                && x.Sender == sender 
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Sent
                                //&& x.Direction == (int)Domain.Models.MessageDirection.OutBound
                                select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find outbound in database");

                record = (from x in db.Messages
                              where
                                x.Recipient == recipient
                                && x.Sender == sender
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Private
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find inbound in database");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageReply()
        {
            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";
            var recipient = "anon";

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();
            var firstMessage = response.Response;

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Message);

            //Ensure first msg is in db
            using (var db = new voatEntities())
            {
                var record = (from x in db.Messages
                              where
                                x.Recipient == recipient
                                && x.Sender == sender
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Sent
                                //&& x.Direction == (int)Domain.Models.MessageDirection.OutBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find outbound in database");

                record = (from x in db.Messages
                          where
                            x.Recipient == recipient
                            && x.Sender == sender
                            && x.Title == id
                            && x.Content == id
                            && x.Subverse == null
                            && x.CommentID == null
                            && x.SubmissionID == null
                            && x.Type == (int)Domain.Models.MessageType.Private
                            //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                          select x).FirstOrDefault();
            }

            TestHelper.SetPrincipal(recipient);

            var replyCmd = new SendMessageReplyCommand(firstMessage.ID, $"Reply to {firstMessage.ID.ToString()}");
            var replyResponse = await replyCmd.Execute();
            var replyMessage = replyResponse.Response;

            Assert.AreEqual(firstMessage.ID, replyMessage.ParentID);

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageToSubverse()
        {
            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";
            var subverse = "unit";

            //ensure v/Unit has moderators
            using (var db = new voatEntities())
            {
                var record = (from x in db.SubverseModerators
                              where
                                x.Subverse == subverse
                              select x).ToList();
                var mod = "anon";
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerators.Add(new Voat.Data.Models.SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                mod = "unit";
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerators.Add(new Voat.Data.Models.SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                db.SaveChanges();
            }

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = $"v/{subverse}",
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Message);

            using (var db = new voatEntities())
            {
                var record = (from x in db.Messages
                              where
                                x.Sender == sender
                                && x.SenderType == (int)Domain.Models.IdentityType.User
                                && x.Recipient == subverse
                                && x.RecipientType == (int)Domain.Models.IdentityType.Subverse
                                && x.Title == id
                                && x.Content == id
                              select x).ToList();
                Assert.IsNotNull(record, "Can not find message in database");
                Assert.AreEqual(2, record.Count, "Expecting 2 PMs");
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageFromSubverse()
        {
            
            var id = Guid.NewGuid().ToString();
            var sender = "unit";
            var recipient = "User100CCP";

            TestHelper.SetPrincipal("unit");

            var message = new Domain.Models.SendMessage()
            {
                Sender = $"v/{sender}",
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Status.ToString());

            using (var db = new voatEntities())
            {
                var record = (from x in db.Messages
                              where
                                x.Sender == sender
                                && x.SenderType == (int)Domain.Models.IdentityType.Subverse
                                && x.Recipient == recipient
                                && x.RecipientType == (int)Domain.Models.IdentityType.User
                                && x.Title == id
                                && x.Content == id
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Messaging"), TestCategory("Command.Messaging"), TestCategory("Process")]
        public async Task SendMessageReply_ToCommentRelatedMention()
        {
            //create a comment and ping another user aka comment mention
            TestHelper.SetPrincipal("TestUser14");
            var c = new CreateCommentCommand(1, null, "This is my comment @TestUser15, do you like it?");
            var r = await c.Execute();
            Assert.IsTrue(r.Success, r.Message);

            //read ping message and reply to it using message gateway
            TestHelper.SetPrincipal("TestUser15");
            var mq = new Domain.Query.QueryMessages(MessageTypeFlag.CommentMention, MessageState.Unread);
            var messages = await mq.ExecuteAsync();
            Assert.IsTrue(messages.Any(), "Didn't return any messages");

            var m = messages.FirstOrDefault(x => x.CommentID == r.Response.ID);
            Assert.IsNotNull(m, "Cant find message");

            var content = "I reply to you from messages";
            var cmd = new SendMessageReplyCommand(m.ID, content);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Status.ToString());

            //ensure comment exists in thread as message gateway should submit a comment based on message type and info
            using (var db = new voatEntities())
            {
                var record = (from x in db.Comments
                              where
                                x.SubmissionID == m.SubmissionID.Value
                                && x.ParentID == m.CommentID.Value
                                && x.Content == content
                                && x.UserName == "TestUser15"
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendCommentNotificationMessage()
        {
            await TestCommentNotification("unit", "UnitTestUser18", "UnitTestUser17");

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendCommentNotificationMessage_AnonSub()
        {
            await TestCommentNotification("anon", "TestUser03", "TestUser04");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendUserMentionNotificationMessage_Anon()
        {
            await TestUserMentionNotification("anon", "UnitTestUser08", "UnitTestUser09", "UnitTestUser11");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendUserMentionNotificationMessage()
        {
            await TestUserMentionNotification("unit", "UnitTestUser05", "UnitTestUser10", "UnitTestUser07");
        }
        
        #region HelperMethods


        public async Task TestCommentNotification(string sub, string user1, string user2)
        {
            var id = Guid.NewGuid().ToString();

            //Post submission as TestUser1
            TestHelper.SetPrincipal(user1);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = sub, Title = "Let's be creative!", Content = "No" });
            var response = await cmd.Execute();
            Assert.IsTrue(response.Success, "Expected post submission to return true");
            var submission = response.Response;
            Assert.IsNotNull(submission, "Expected a non-null submission response");

            //Reply to comment as TestUser2
            TestHelper.SetPrincipal(user2);
            var commentCmd = new CreateCommentCommand(submission.ID, null, "Important Comment");
            var responseComment = await commentCmd.Execute();
            Assert.IsTrue(responseComment.Success, "Expected post comment to return true");
            var comment = responseComment.Response;
            Assert.IsNotNull(comment, "Expected a non-null comment response");


            //Check to see if Comment notification exists in messages
            using (var db = new voatEntities())
            {
                var record = (from x in db.Messages
                              where
                                x.Recipient == user1
                                && x.Sender == user2
                                && x.IsAnonymized == submission.IsAnonymized
                                && x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == comment.ID
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.SubmissionReply
                               // && x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }


        public async Task TestUserMentionNotification(string sub, string user1, string user2, string user3)
        {
            var id = Guid.NewGuid().ToString();
            //var user1 = "UnitTestUser10";
            //var user2 = "UnitTestUser20";

            


            //Post submission as TestUser1
            TestHelper.SetPrincipal(user1);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = sub, Title = "I love you more than butter in my coffee", Content = $"Did you hear that @{user2} or /u/{user2}?" });
            var response = await cmd.Execute();
            Assert.IsTrue(response.Success, "Expected post submission to return true");
            var submission = response.Response;
            Assert.IsNotNull(submission, "Expected a non-null submission response");


            //Reply to comment as TestUser2
            TestHelper.SetPrincipal(user2);
            var commentCmd = new CreateCommentCommand(submission.ID, null, $"I bet @{user3} could think of something!");
            var responseComment = await commentCmd.Execute();
            Assert.IsTrue(responseComment.Success, "Expected post comment to return true");
            var comment = responseComment.Response;
            Assert.IsNotNull(comment, "Expected a non-null comment response");

            //Check to see if Comment notification exists in messages
            using (var db = new voatEntities())
            {
                var subverse = (from x in db.Subverses
                                where x.Name.Equals(sub, StringComparison.OrdinalIgnoreCase)
                                select x).FirstOrDefault();

                Assert.AreEqual(submission.IsAnonymized, subverse.IsAnonymized, "Expecting matching anon settings");
                Assert.AreEqual(comment.IsAnonymized, subverse.IsAnonymized, "Expecting matching anon settings comment");

                //test for submission notification
                var record = (from x in db.Messages
                              where
                                x.Recipient == user2
                                && x.Sender == user1
                                && x.IsAnonymized == submission.IsAnonymized
                                //&& x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == null
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.SubmissionMention
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find submission message in database");

                //test for comment notification
                record = (from x in db.Messages
                              where
                                x.Recipient == user3
                                && x.Sender == user2
                                && x.IsAnonymized == submission.IsAnonymized
                                && x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == comment.ID
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.CommentMention
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find comment message in database");
            }
        }

        #endregion
    }
}
