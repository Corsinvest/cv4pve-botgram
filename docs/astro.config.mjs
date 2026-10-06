// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import corsinvestTheme from '@corsinvest/cv4pve-docs-theme';

export default defineConfig({
  site: 'https://corsinvest.github.io',
  base: '/cv4pve-botgram',
  integrations: [
    starlight({
      title: 'cv4pve-botgram',
      description: 'Telegram bot to manage and monitor a Proxmox VE cluster from a phone.',
      // Brand, product icon, GitHub link, the Corsinvest sidebar group and
      // external links in a new tab come from the shared cv4pve theme.
      plugins: [
        corsinvestTheme({
          repo: 'cv4pve-botgram',
          // Product icon: favicon and header, dark variant for the dark theme.
          icon: { light: '/icon.svg', dark: '/icon-dark.svg' },
          // Button in the home hero: the same engine runs inside cv4pve-admin.
          admin: { module: 'bots' },
          // Visits, without cookies.
          matomo: { url: 'https://matomo.corsinvest.it/', siteId: 14 },
          // Steps panel in the home hero: the first four steps of Getting started, in the same
          // order and words. The commands are in the pages (CliInstall).
          steps: {
            items: [
              'Install cv4pve-botgram',
              { text: 'Create a Telegram bot', href: 'telegram-bot/' },
              { text: 'Create an API token', href: 'permissions/#user-and-token' },
              'Run `cv4pve-botgram`',
            ],
          },
        }),
      ],
      sidebar: [
        {
          label: 'Start here',
          items: ['getting-started', 'permissions', 'connection', 'troubleshooting'],
        },
        {
          label: 'Integration',
          items: ['telegram-bot', 'service'],
        },
        {
          label: 'Commands',
          items: [
            { label: 'Overview', slug: 'commands' },
            'commands/api',
            'commands/aliases',
          ],
        },
      ],
    }),
  ],
});
