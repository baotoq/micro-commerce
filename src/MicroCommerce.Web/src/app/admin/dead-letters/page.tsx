'use client';

import { useState, useEffect, useCallback } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertTriangle, RotateCcw, Trash2, Inbox } from 'lucide-react';
import { toast } from 'sonner';
import {
  getDeadLetterMessages,
  retryDeadLetterMessage,
  purgeDeadLetterMessages,
  DeadLetterMessageDto,
} from '@/lib/api';

export default function DeadLettersPage() {
  const [messages, setMessages] = useState<DeadLetterMessageDto[]>([]);
  const [queueNames, setQueueNames] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedQueue, setSelectedQueue] = useState<string | null>(null);
  const [retryingId, setRetryingId] = useState<number | null>(null);
  const [purging, setPurging] = useState(false);

  const fetchMessages = useCallback(async () => {
    try {
      const data = await getDeadLetterMessages(selectedQueue ?? undefined);
      setMessages(data.messages);
      setQueueNames(data.queueNames);
    } catch (error) {
      console.error('Failed to fetch dead-letter messages:', error);
      toast.error('Failed to load dead-letter messages');
    } finally {
      setLoading(false);
    }
  }, [selectedQueue]);

  useEffect(() => {
    setLoading(true);
    fetchMessages();
  }, [fetchMessages]);

  // Auto-refresh every 30 seconds
  useEffect(() => {
    const interval = setInterval(() => {
      fetchMessages();
    }, 30000);
    return () => clearInterval(interval);
  }, [fetchMessages]);

  const handleRetry = async (message: DeadLetterMessageDto) => {
    setRetryingId(message.sequenceNumber);
    try {
      await retryDeadLetterMessage(message.queueName, message.sequenceNumber);
      toast.success('Message re-queued for processing');
      await fetchMessages();
    } catch (error) {
      console.error('Failed to retry message:', error);
      toast.error('Failed to retry message');
    } finally {
      setRetryingId(null);
    }
  };

  const handlePurge = async () => {
    if (!selectedQueue) return;
    if (!window.confirm(`Purge all dead-lettered messages from "${selectedQueue}"?`)) return;

    setPurging(true);
    try {
      const count = await purgeDeadLetterMessages(selectedQueue);
      toast.success(`Purged ${count} messages`);
      await fetchMessages();
    } catch (error) {
      console.error('Failed to purge messages:', error);
      toast.error('Failed to purge messages');
    } finally {
      setPurging(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const truncate = (text: string, maxLength: number) => {
    if (text.length <= maxLength) return text;
    return text.slice(0, maxLength) + '...';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dead Letter Queue</h1>
          <p className="text-gray-500">Messages that failed processing after retries</p>
        </div>
        <div className="flex items-center gap-3">
          <select
            value={selectedQueue ?? ''}
            onChange={(e) => setSelectedQueue(e.target.value || null)}
            className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Queues</option>
            {queueNames.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </select>
          <Button
            variant="destructive"
            size="sm"
            onClick={handlePurge}
            disabled={!selectedQueue || purging}
          >
            <Trash2 className="mr-2 h-4 w-4" />
            {purging ? 'Purging...' : 'Purge All'}
          </Button>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-lg border">
        {loading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex items-center space-x-4">
                <div className="space-y-2 flex-1">
                  <Skeleton className="h-4 w-[200px]" />
                  <Skeleton className="h-4 w-[400px]" />
                </div>
              </div>
            ))}
          </div>
        ) : messages.length > 0 ? (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Message Type</TableHead>
                <TableHead>Error Details</TableHead>
                <TableHead>Correlation ID</TableHead>
                <TableHead>Enqueued Time</TableHead>
                <TableHead>Queue Name</TableHead>
                <TableHead className="w-[80px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {messages.map((message) => (
                <TableRow key={`${message.queueName}-${message.sequenceNumber}`}>
                  <TableCell className="font-medium">{message.messageType}</TableCell>
                  <TableCell
                    className="text-gray-500 max-w-[300px]"
                    title={message.errorDescription}
                  >
                    {truncate(message.errorDescription, 100)}
                  </TableCell>
                  <TableCell className="font-mono text-xs text-gray-500">
                    {message.correlationId || '\u2014'}
                  </TableCell>
                  <TableCell className="text-gray-500">
                    {formatDate(message.enqueuedTime)}
                  </TableCell>
                  <TableCell className="text-gray-500">{message.queueName}</TableCell>
                  <TableCell>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleRetry(message)}
                      disabled={retryingId === message.sequenceNumber}
                    >
                      <RotateCcw className="mr-1 h-3 w-3" />
                      Retry
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : (
          <div className="p-12 text-center">
            <div className="text-gray-400 mb-4">
              <Inbox className="h-12 w-12 mx-auto" />
            </div>
            <h3 className="text-lg font-medium text-gray-900">No dead-lettered messages</h3>
            <p className="text-gray-500 mt-1">
              All messages are being processed successfully
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
