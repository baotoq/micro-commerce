using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroCommerce.ApiService.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConventionsDryConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                schema: "outbox",
                table: "OutboxMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                schema: "outbox",
                table: "OutboxMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxState",
                schema: "outbox",
                table: "OutboxState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessage",
                schema: "outbox",
                table: "OutboxMessage");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                schema: "outbox",
                table: "InboxState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InboxState",
                schema: "outbox",
                table: "InboxState");

            migrationBuilder.RenameTable(
                name: "OutboxState",
                schema: "outbox",
                newName: "outbox_state",
                newSchema: "outbox");

            migrationBuilder.RenameTable(
                name: "OutboxMessage",
                schema: "outbox",
                newName: "outbox_message",
                newSchema: "outbox");

            migrationBuilder.RenameTable(
                name: "InboxState",
                schema: "outbox",
                newName: "inbox_state",
                newSchema: "outbox");

            migrationBuilder.RenameColumn(
                name: "Delivered",
                schema: "outbox",
                table: "outbox_state",
                newName: "delivered");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "outbox",
                table: "outbox_state",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                schema: "outbox",
                table: "outbox_state",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "LockId",
                schema: "outbox",
                table: "outbox_state",
                newName: "lock_id");

            migrationBuilder.RenameColumn(
                name: "LastSequenceNumber",
                schema: "outbox",
                table: "outbox_state",
                newName: "last_sequence_number");

            migrationBuilder.RenameColumn(
                name: "OutboxId",
                schema: "outbox",
                table: "outbox_state",
                newName: "outbox_id");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxState_Created",
                schema: "outbox",
                table: "outbox_state",
                newName: "ix_outbox_state_created");

            migrationBuilder.RenameColumn(
                name: "Properties",
                schema: "outbox",
                table: "outbox_message",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Headers",
                schema: "outbox",
                table: "outbox_message",
                newName: "headers");

            migrationBuilder.RenameColumn(
                name: "Body",
                schema: "outbox",
                table: "outbox_message",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "SourceAddress",
                schema: "outbox",
                table: "outbox_message",
                newName: "source_address");

            migrationBuilder.RenameColumn(
                name: "SentTime",
                schema: "outbox",
                table: "outbox_message",
                newName: "sent_time");

            migrationBuilder.RenameColumn(
                name: "ResponseAddress",
                schema: "outbox",
                table: "outbox_message",
                newName: "response_address");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                schema: "outbox",
                table: "outbox_message",
                newName: "request_id");

            migrationBuilder.RenameColumn(
                name: "OutboxId",
                schema: "outbox",
                table: "outbox_message",
                newName: "outbox_id");

            migrationBuilder.RenameColumn(
                name: "MessageType",
                schema: "outbox",
                table: "outbox_message",
                newName: "message_type");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                schema: "outbox",
                table: "outbox_message",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "InitiatorId",
                schema: "outbox",
                table: "outbox_message",
                newName: "initiator_id");

            migrationBuilder.RenameColumn(
                name: "InboxMessageId",
                schema: "outbox",
                table: "outbox_message",
                newName: "inbox_message_id");

            migrationBuilder.RenameColumn(
                name: "InboxConsumerId",
                schema: "outbox",
                table: "outbox_message",
                newName: "inbox_consumer_id");

            migrationBuilder.RenameColumn(
                name: "FaultAddress",
                schema: "outbox",
                table: "outbox_message",
                newName: "fault_address");

            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                schema: "outbox",
                table: "outbox_message",
                newName: "expiration_time");

            migrationBuilder.RenameColumn(
                name: "EnqueueTime",
                schema: "outbox",
                table: "outbox_message",
                newName: "enqueue_time");

            migrationBuilder.RenameColumn(
                name: "DestinationAddress",
                schema: "outbox",
                table: "outbox_message",
                newName: "destination_address");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                schema: "outbox",
                table: "outbox_message",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "ConversationId",
                schema: "outbox",
                table: "outbox_message",
                newName: "conversation_id");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                schema: "outbox",
                table: "outbox_message",
                newName: "content_type");

            migrationBuilder.RenameColumn(
                name: "SequenceNumber",
                schema: "outbox",
                table: "outbox_message",
                newName: "sequence_number");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                schema: "outbox",
                table: "outbox_message",
                newName: "ix_outbox_message_outbox_id_sequence_number");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "outbox",
                table: "outbox_message",
                newName: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                schema: "outbox",
                table: "outbox_message",
                newName: "ix_outbox_message_expiration_time");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                schema: "outbox",
                table: "outbox_message",
                newName: "ix_outbox_message_enqueue_time");

            migrationBuilder.RenameColumn(
                name: "Received",
                schema: "outbox",
                table: "inbox_state",
                newName: "received");

            migrationBuilder.RenameColumn(
                name: "Delivered",
                schema: "outbox",
                table: "inbox_state",
                newName: "delivered");

            migrationBuilder.RenameColumn(
                name: "Consumed",
                schema: "outbox",
                table: "inbox_state",
                newName: "consumed");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "outbox",
                table: "inbox_state",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                schema: "outbox",
                table: "inbox_state",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "ReceiveCount",
                schema: "outbox",
                table: "inbox_state",
                newName: "receive_count");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                schema: "outbox",
                table: "inbox_state",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "LockId",
                schema: "outbox",
                table: "inbox_state",
                newName: "lock_id");

            migrationBuilder.RenameColumn(
                name: "LastSequenceNumber",
                schema: "outbox",
                table: "inbox_state",
                newName: "last_sequence_number");

            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                schema: "outbox",
                table: "inbox_state",
                newName: "expiration_time");

            migrationBuilder.RenameColumn(
                name: "ConsumerId",
                schema: "outbox",
                table: "inbox_state",
                newName: "consumer_id");

            migrationBuilder.RenameIndex(
                name: "IX_InboxState_Delivered",
                schema: "outbox",
                table: "inbox_state",
                newName: "ix_inbox_state_delivered");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_state",
                schema: "outbox",
                table: "outbox_state",
                column: "outbox_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_message",
                schema: "outbox",
                table: "outbox_message",
                column: "sequence_number");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_inbox_state_message_id_consumer_id",
                schema: "outbox",
                table: "inbox_state",
                columns: new[] { "message_id", "consumer_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_inbox_state",
                schema: "outbox",
                table: "inbox_state",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_",
                schema: "outbox",
                table: "outbox_message",
                columns: new[] { "inbox_message_id", "inbox_consumer_id" },
                principalSchema: "outbox",
                principalTable: "inbox_state",
                principalColumns: new[] { "message_id", "consumer_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_outbox_message_outbox_state_outbox_id",
                schema: "outbox",
                table: "outbox_message",
                column: "outbox_id",
                principalSchema: "outbox",
                principalTable: "outbox_state",
                principalColumn: "outbox_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_",
                schema: "outbox",
                table: "outbox_message");

            migrationBuilder.DropForeignKey(
                name: "fk_outbox_message_outbox_state_outbox_id",
                schema: "outbox",
                table: "outbox_message");

            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_state",
                schema: "outbox",
                table: "outbox_state");

            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_message",
                schema: "outbox",
                table: "outbox_message");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_inbox_state_message_id_consumer_id",
                schema: "outbox",
                table: "inbox_state");

            migrationBuilder.DropPrimaryKey(
                name: "pk_inbox_state",
                schema: "outbox",
                table: "inbox_state");

            migrationBuilder.RenameTable(
                name: "outbox_state",
                schema: "outbox",
                newName: "OutboxState",
                newSchema: "outbox");

            migrationBuilder.RenameTable(
                name: "outbox_message",
                schema: "outbox",
                newName: "OutboxMessage",
                newSchema: "outbox");

            migrationBuilder.RenameTable(
                name: "inbox_state",
                schema: "outbox",
                newName: "InboxState",
                newSchema: "outbox");

            migrationBuilder.RenameColumn(
                name: "delivered",
                schema: "outbox",
                table: "OutboxState",
                newName: "Delivered");

            migrationBuilder.RenameColumn(
                name: "created",
                schema: "outbox",
                table: "OutboxState",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "row_version",
                schema: "outbox",
                table: "OutboxState",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "lock_id",
                schema: "outbox",
                table: "OutboxState",
                newName: "LockId");

            migrationBuilder.RenameColumn(
                name: "last_sequence_number",
                schema: "outbox",
                table: "OutboxState",
                newName: "LastSequenceNumber");

            migrationBuilder.RenameColumn(
                name: "outbox_id",
                schema: "outbox",
                table: "OutboxState",
                newName: "OutboxId");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_state_created",
                schema: "outbox",
                table: "OutboxState",
                newName: "IX_OutboxState_Created");

            migrationBuilder.RenameColumn(
                name: "properties",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "headers",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "Headers");

            migrationBuilder.RenameColumn(
                name: "body",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "source_address",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "SourceAddress");

            migrationBuilder.RenameColumn(
                name: "sent_time",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "SentTime");

            migrationBuilder.RenameColumn(
                name: "response_address",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "ResponseAddress");

            migrationBuilder.RenameColumn(
                name: "request_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "outbox_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "OutboxId");

            migrationBuilder.RenameColumn(
                name: "message_type",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "MessageType");

            migrationBuilder.RenameColumn(
                name: "message_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "initiator_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "InitiatorId");

            migrationBuilder.RenameColumn(
                name: "inbox_message_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "InboxMessageId");

            migrationBuilder.RenameColumn(
                name: "inbox_consumer_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "InboxConsumerId");

            migrationBuilder.RenameColumn(
                name: "fault_address",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "FaultAddress");

            migrationBuilder.RenameColumn(
                name: "expiration_time",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "ExpirationTime");

            migrationBuilder.RenameColumn(
                name: "enqueue_time",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "EnqueueTime");

            migrationBuilder.RenameColumn(
                name: "destination_address",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "DestinationAddress");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "CorrelationId");

            migrationBuilder.RenameColumn(
                name: "conversation_id",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "ConversationId");

            migrationBuilder.RenameColumn(
                name: "content_type",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "ContentType");

            migrationBuilder.RenameColumn(
                name: "sequence_number",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_outbox_id_sequence_number",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_OutboxId_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_expiration_time",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_ExpirationTime");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_message_enqueue_time",
                schema: "outbox",
                table: "OutboxMessage",
                newName: "IX_OutboxMessage_EnqueueTime");

            migrationBuilder.RenameColumn(
                name: "received",
                schema: "outbox",
                table: "InboxState",
                newName: "Received");

            migrationBuilder.RenameColumn(
                name: "delivered",
                schema: "outbox",
                table: "InboxState",
                newName: "Delivered");

            migrationBuilder.RenameColumn(
                name: "consumed",
                schema: "outbox",
                table: "InboxState",
                newName: "Consumed");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "outbox",
                table: "InboxState",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "row_version",
                schema: "outbox",
                table: "InboxState",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "receive_count",
                schema: "outbox",
                table: "InboxState",
                newName: "ReceiveCount");

            migrationBuilder.RenameColumn(
                name: "message_id",
                schema: "outbox",
                table: "InboxState",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "lock_id",
                schema: "outbox",
                table: "InboxState",
                newName: "LockId");

            migrationBuilder.RenameColumn(
                name: "last_sequence_number",
                schema: "outbox",
                table: "InboxState",
                newName: "LastSequenceNumber");

            migrationBuilder.RenameColumn(
                name: "expiration_time",
                schema: "outbox",
                table: "InboxState",
                newName: "ExpirationTime");

            migrationBuilder.RenameColumn(
                name: "consumer_id",
                schema: "outbox",
                table: "InboxState",
                newName: "ConsumerId");

            migrationBuilder.RenameIndex(
                name: "ix_inbox_state_delivered",
                schema: "outbox",
                table: "InboxState",
                newName: "IX_InboxState_Delivered");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxState",
                schema: "outbox",
                table: "OutboxState",
                column: "OutboxId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessage",
                schema: "outbox",
                table: "OutboxMessage",
                column: "SequenceNumber");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_InboxState_MessageId_ConsumerId",
                schema: "outbox",
                table: "InboxState",
                columns: new[] { "MessageId", "ConsumerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InboxState",
                schema: "outbox",
                table: "InboxState",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                schema: "outbox",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalSchema: "outbox",
                principalTable: "InboxState",
                principalColumns: new[] { "MessageId", "ConsumerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                schema: "outbox",
                table: "OutboxMessage",
                column: "OutboxId",
                principalSchema: "outbox",
                principalTable: "OutboxState",
                principalColumn: "OutboxId");
        }
    }
}
