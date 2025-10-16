import { z } from 'zod';

export const serviceSchema = z.object({
  name: z.string().min(1),
  description: z.string().min(1),
  durationMinutes: z.number().int().positive(),
  price: z.number().min(0),
});