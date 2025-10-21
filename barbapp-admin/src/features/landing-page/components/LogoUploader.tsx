import React, { useRef, useState, useCallback } from 'react';
import { Upload, X, Image as ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useLogoUpload } from '../hooks/useLogoUpload';
import { LOGO_UPLOAD_CONFIG } from '../constants/validation';
import type { LogoUploaderProps } from '../types/landing-page.types';

export const LogoUploader: React.FC<LogoUploaderProps> = ({
  barbershopId,
  currentLogoUrl,
  onUploadComplete,
  disabled = false,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragOver, setIsDragOver] = useState(false);

  const {
    isUploading,
    validationError,
    previewUrl,
    uploadLogo,
    createPreview,
    clearPreview,
  } = useLogoUpload(barbershopId);

  // Display URL: preview > current logo > null
  const displayUrl = previewUrl || currentLogoUrl;

  const handleFileSelect = useCallback((file: File) => {
    if (disabled || isUploading) return;

    // Create preview first
    createPreview(file);

    // Upload the file
    uploadLogo(file);

    // Call completion callback if provided
    if (onUploadComplete) {
      // Note: onUploadComplete would be called from the hook's onSuccess
      // This is just for interface consistency
    }
  }, [disabled, isUploading, createPreview, uploadLogo, onUploadComplete]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
    // Reset input value to allow re-uploading the same file
    e.target.value = '';
  };

  const handleDrop = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(false);

    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0) {
      handleFileSelect(files[0]);
    }
  }, [handleFileSelect]);

  const handleDragOver = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    if (!disabled && !isUploading) {
      setIsDragOver(true);
    }
  }, [disabled, isUploading]);

  const handleDragLeave = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragOver(false);
  }, []);

  const handleClick = () => {
    if (!disabled && !isUploading) {
      fileInputRef.current?.click();
    }
  };

  const handleRemove = () => {
    if (!disabled && !isUploading) {
      clearPreview();
      // Note: Logo removal would be handled by a separate delete function
      // For now, just clear preview. Actual deletion should be implemented
      // in the parent component or hook
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <div className="space-y-4">
      <label className="block text-sm font-medium text-gray-700">
        Logo da Barbearia
      </label>

      <div className="flex items-start gap-6">
        {/* Upload Area */}
        <div
          className={`
            relative w-32 h-32 border-2 border-dashed rounded-lg overflow-hidden transition-all duration-200 cursor-pointer
            ${isDragOver ? 'border-primary bg-primary/5 scale-105' : 'border-gray-300 hover:border-gray-400'}
            ${disabled || isUploading ? 'opacity-50 cursor-not-allowed' : ''}
            ${displayUrl ? 'border-solid' : ''}
          `}
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onClick={handleClick}
        >
          {displayUrl ? (
            <>
              <img
                src={displayUrl}
                alt="Logo preview"
                className="w-full h-full object-cover"
              />
              {!disabled && !isUploading && (
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleRemove();
                  }}
                  className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 hover:bg-red-600 transition-colors"
                  title="Remover logo"
                >
                  <X size={14} />
                </button>
              )}
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-full text-gray-400">
              {isUploading ? (
                <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div>
              ) : (
                <>
                  <ImageIcon size={32} />
                  <span className="text-xs mt-1">Logo</span>
                </>
              )}
            </div>
          )}

          {/* Drag overlay */}
          {isDragOver && (
            <div className="absolute inset-0 bg-primary/10 flex items-center justify-center">
              <div className="text-center text-primary">
                <Upload size={24} className="mx-auto mb-1" />
                <span className="text-sm font-medium">Solte para fazer upload</span>
              </div>
            </div>
          )}
        </div>

        {/* Controls and Info */}
        <div className="flex-1 min-w-0">
          <input
            ref={fileInputRef}
            type="file"
            accept={LOGO_UPLOAD_CONFIG.allowedTypes.join(',')}
            onChange={handleInputChange}
            className="hidden"
            disabled={disabled || isUploading}
            data-testid="logo-file-input"
          />

          <Button
            type="button"
            variant="outline"
            onClick={handleClick}
            disabled={disabled || isUploading}
            className="mb-3"
          >
            {isUploading ? 'Enviando...' : displayUrl ? 'Alterar Logo' : 'Fazer Upload'}
          </Button>

          <div className="text-sm text-gray-600 space-y-1">
            <p>
              <strong>Formatos aceitos:</strong> JPG, PNG, SVG, WebP
            </p>
            <p>
              <strong>Tamanho máximo:</strong> {formatFileSize(LOGO_UPLOAD_CONFIG.maxSize)}
            </p>
            <p>
              <strong>Tamanho recomendado:</strong> {LOGO_UPLOAD_CONFIG.recommendedSize.width}x{LOGO_UPLOAD_CONFIG.recommendedSize.height}px
            </p>
          </div>

          {/* Error Message */}
          {validationError && (
            <div className="mt-3 p-3 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-600">{validationError.message}</p>
            </div>
          )}

          {/* Success Message */}
          {displayUrl && !isUploading && !validationError && (
            <div className="mt-3 p-3 bg-green-50 border border-green-200 rounded-md">
              <p className="text-sm text-green-600">
                {previewUrl ? 'Preview carregado. Salvando...' : 'Logo atualizado com sucesso!'}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};