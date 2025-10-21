import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { LogoUploader } from '../LogoUploader';
import { useLogoUpload } from '../../hooks/useLogoUpload';
import '@testing-library/jest-dom';

// Mock the hook
vi.mock('../../hooks/useLogoUpload', () => ({
  useLogoUpload: vi.fn(),
}));

const mockUseLogoUpload = vi.mocked(useLogoUpload);

describe('LogoUploader', () => {
  const mockBarbershopId = 'test-barbershop-id';
  const mockCurrentLogoUrl = 'https://example.com/logo.png';

  const defaultMockReturn = {
    isUploading: false,
    isDeleting: false,
    uploadError: null,
    deleteError: null,
    validationError: null,
    previewUrl: null,
    uploadLogo: vi.fn(),
    deleteLogo: vi.fn(),
    createPreview: vi.fn(),
    clearPreview: vi.fn(),
    validateFile: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseLogoUpload.mockReturnValue(defaultMockReturn);
  });

  it('renders with current logo', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={mockCurrentLogoUrl}
      />
    );

    expect(screen.getByText('Logo da Barbearia')).toBeInTheDocument();
    expect(screen.getByAltText('Logo preview')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /alterar logo/i })).toBeInTheDocument();
  });

  it('renders without logo', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    expect(screen.getByText('Logo da Barbearia')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /fazer upload/i })).toBeInTheDocument();
  });

  it('shows upload button when no logo', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    const uploadButton = screen.getByRole('button', { name: /fazer upload/i });
    expect(uploadButton).toBeInTheDocument();
  });

  it('shows loading state during upload', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
      isUploading: true,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    expect(screen.getByText('Enviando...')).toBeInTheDocument();
  });

  it('shows validation error', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
      validationError: {
        type: 'size',
        message: 'Arquivo muito grande',
      },
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    expect(screen.getByText('Arquivo muito grande')).toBeInTheDocument();
  });

  it('shows preview when available', () => {
    const mockPreviewUrl = 'blob:preview-url';
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
      previewUrl: mockPreviewUrl,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={mockCurrentLogoUrl}
      />
    );

    const img = screen.getByAltText('Logo preview');
    expect(img).toHaveAttribute('src', mockPreviewUrl);
  });

  it('disables when disabled prop is true', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
        disabled={true}
      />
    );

    const uploadButton = screen.getByRole('button', { name: /fazer upload/i });
    expect(uploadButton).toBeDisabled();
  });

  it('calls uploadLogo when file is selected', async () => {
    const mockUploadLogo = vi.fn();
    const mockCreatePreview = vi.fn();
    const mockFile = new File(['test'], 'test.png', { type: 'image/png' });

    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
      uploadLogo: mockUploadLogo,
      createPreview: mockCreatePreview,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    const input = screen.getByTestId('logo-file-input');
    fireEvent.change(input, { target: { files: [mockFile] } });

    await waitFor(() => {
      expect(mockCreatePreview).toHaveBeenCalledWith(mockFile);
      expect(mockUploadLogo).toHaveBeenCalledWith(mockFile);
    });
  });

  it('shows file format and size information', () => {
    mockUseLogoUpload.mockReturnValue({
      ...defaultMockReturn,
    });

    render(
      <LogoUploader
        barbershopId={mockBarbershopId}
        currentLogoUrl={undefined}
      />
    );

    expect(screen.getByText(/formatos aceitos:/i)).toBeInTheDocument();
    expect(screen.getByText(/tamanho máximo:/i)).toBeInTheDocument();
    expect(screen.getByText(/tamanho recomendado:/i)).toBeInTheDocument();
  });
});