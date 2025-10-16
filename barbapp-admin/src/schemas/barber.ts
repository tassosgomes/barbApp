import { z } from 'zod';

export const createBarberSchema = z.object({
  name: z.string().min(1).max(100),
  email: z.string().email(),
  password: z.string().min(8),
  phone: z.string().regex(/^\(\d{2}\) \d{4,5}-\d{4}$/),
  serviceIds: z.array(z.string().uuid()).min(1),
});

export const updateBarberSchema = createBarberSchema.omit({ email: true, password: true });