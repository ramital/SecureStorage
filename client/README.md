# Patient Management System

The PHI Secure Storage UI a lightweight React front-end designed to interact with the SecureStorage API for managing encrypted healthcare data. It enables healthcare admins or authorized users to upload, retrieve, and decrypt patient-related records securely through a user-friendly interface.

🔐 Built with compliance in mind, the UI enforces access control (e.g., via JWT and OpenFGA), supports multi-tenant environments, and integrates seamlessly with backend services like Azure Blob Storage and Key Vault.

🏥 Use Case: A small clinic can use this UI to manage encrypted ePHI files without needing complex IT infrastructure—enabling secure patient data workflows in line with HIPAA guidelines.

## Features

- Secure authentication system
- Patient record management
- Advanced search functionality
- Biometric data handling
- Real-time filtering
- Responsive design

## Tech Stack

- React 19
- Vite 6
- React Router DOM 6
- Axios for API communication
- HeroIcons for UI elements

## Getting Started

1. Clone the repository
2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

4. The application will be available at `http://localhost:5173`

## Available Scripts

- `npm run dev` - Starts the development server
- `npm run build` - Builds the app for production
- `npm run preview` - Preview the production build
- `npm run lint` - Run ESLint for code quality

## Project Structure

```
src/
  ├── assets/      # Static assets
  ├── components/  # Reusable React components
  ├── pages/       # Page components
  ├── utils/       # Helper functions and constants
  └── App.jsx      # Main application component
```

## Security Features

- JWT-based authentication
- Protected routes
- Secure storage of sensitive information
- Session management

## API Integration

The application connects to a REST API at `localhost:32773` for all data operations. Make sure the API server is running before starting the application.


## ℹ️ License

MIT. LICENSE.

## ℹ️  Proof-of-Concept (PoC) Code
This repository contains the PoC implementation of the Healthcare PHI Secure Storage UI, developed by Ahmad Rami El Tal.
