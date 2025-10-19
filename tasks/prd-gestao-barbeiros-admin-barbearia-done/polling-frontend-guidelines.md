# Frontend Polling Guidelines - Team Schedule (30s)

## Overview
The team schedule endpoint (`GET /api/barbers/schedule`) should be polled every 30 seconds to keep the admin barbearia's view updated with the latest appointments.

## Implementation Requirements

### Polling Configuration
- **Interval**: 30 seconds
- **Endpoint**: `GET /api/barbers/schedule`
- **Query Parameters**:
  - `date` (optional): Date in YYYY-MM-DD format. Defaults to today if not provided.
  - `barberId` (optional): Filter by specific barber ID.

### Polling Lifecycle
1. **Start Polling**: Begin polling when the admin enters the team schedule screen
2. **Stop Polling**: Cancel polling when the admin leaves the screen (navigation, component unmount)
3. **Error Handling**: Implement exponential backoff on errors (max 5 retries, then show error state)

### React Implementation Example

```typescript
import { useEffect, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';

interface TeamScheduleData {
  appointments: Array<{
    id: string;
    barberId: string;
    barberName: string;
    customerId: string;
    customerName: string;
    startTime: string;
    endTime: string;
    serviceName: string;
    status: string;
  }>;
}

export const useTeamSchedulePolling = (date?: string, barberId?: string) => {
  const [isPolling, setIsPolling] = useState(true);
  const abortControllerRef = useRef<AbortController>();

  const query = useQuery({
    queryKey: ['team-schedule', date, barberId],
    queryFn: async ({ signal }) => {
      const params = new URLSearchParams();
      if (date) params.append('date', date);
      if (barberId) params.append('barberId', barberId);

      const response = await fetch(`/api/barbers/schedule?${params}`, {
        signal,
        headers: {
          'Authorization': `Bearer ${getAuthToken()}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to fetch team schedule');
      }

      return response.json() as Promise<TeamScheduleData>;
    },
    refetchInterval: isPolling ? 30000 : false, // 30 seconds
    refetchIntervalInBackground: false,
    retry: (failureCount, error) => {
      // Exponential backoff: retry up to 5 times
      return failureCount < 5;
    },
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });

  // Cleanup on unmount or when polling stops
  useEffect(() => {
    return () => {
      abortControllerRef.current?.abort();
    };
  }, []);

  const stopPolling = () => {
    setIsPolling(false);
    abortControllerRef.current?.abort();
  };

  const startPolling = () => {
    setIsPolling(true);
  };

  return {
    ...query,
    stopPolling,
    startPolling,
    isPolling,
  };
};
```

### Usage in Component

```typescript
import React, { useEffect } from 'react';
import { useTeamSchedulePolling } from './hooks/useTeamSchedulePolling';

export const TeamScheduleView: React.FC = () => {
  const { data, isLoading, error, stopPolling } = useTeamSchedulePolling();

  // Stop polling when component unmounts
  useEffect(() => {
    return () => {
      stopPolling();
    };
  }, [stopPolling]);

  if (isLoading) return <div>Loading schedule...</div>;
  if (error) return <div>Error loading schedule</div>;

  return (
    <div>
      <h1>Team Schedule</h1>
      {/* Render schedule data */}
      {data?.appointments.map(appointment => (
        <div key={appointment.id}>
          {appointment.barberName} - {appointment.customerName} - {appointment.serviceName}
        </div>
      ))}
    </div>
  );
};
```

## Error Handling
- **Network Errors**: Show user-friendly message, continue polling with backoff
- **Authentication Errors**: Redirect to login, stop polling
- **Server Errors (5xx)**: Retry with backoff, show loading state
- **Client Errors (4xx)**: Stop polling, show error message

## Performance Considerations
- Polling only occurs when the screen is active
- Use AbortController to cancel in-flight requests when leaving the screen
- Consider implementing visibility API to pause polling when tab is not visible
- Cache responses locally to reduce perceived loading time

## Testing
- Test polling starts/stops correctly
- Test error handling and retry logic
- Test component unmounting cancels polling
- Mock the API responses for consistent testing