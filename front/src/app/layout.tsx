import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import '@/styles/globals.css';
import { Providers } from '@/components/providers';
import { DirectionSetter } from '@/components/shared/DirectionSetter';

const inter = Inter({ subsets: ['latin'], variable: '--font-sans', display: 'swap' });

export const metadata: Metadata = {
  title: { default: 'Payment Admin', template: '%s | Payment Admin' },
  description: 'Minisource Payment Service Operations Console',
};

export const viewport: Viewport = {
  themeColor: [{ media: '(prefers-color-scheme: light)', color: 'white' }, { media: '(prefers-color-scheme: dark)', color: 'black' }],
  width: 'device-width', initialScale: 1, maximumScale: 1,
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} font-sans antialiased`}>
        <Providers>
          <DirectionSetter />
          {children}
        </Providers>
      </body>
    </html>
  );
}
