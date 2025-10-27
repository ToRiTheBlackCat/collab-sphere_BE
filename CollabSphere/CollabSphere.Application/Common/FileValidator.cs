﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public static class FileValidator
    {
        // Allowed extensions (whitelist)
        private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Documents
            ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt",
            // Spreadsheets
            ".xls", ".xlsx", ".csv", ".ods",
            // Presentations
            ".ppt", ".pptx", ".odp",
            // Images
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            // Archives (optional)
            ".zip", ".rar", ".7z",
            // Code files (optional)
            ".py", ".js", ".java", ".cpp", ".cs", ".html", ".css"
        };

        // Allowed MIME types
        private static readonly HashSet<string> _allowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain",
            "text/csv",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp",
            "application/zip",
            "application/x-rar-compressed",
            "application/x-7z-compressed",
            "text/html",
            "text/css",
            "text/javascript"
        };

        // File signature map (for common file types)
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
        {
            { ".jpg",  new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png",  new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
            { ".gif",  new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".pdf",  new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
            { ".zip",  new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".rar",  new List<byte[]> { new byte[] { 0x52, 0x61, 0x72, 0x21 } } }, // Rar!
            { ".7z",   new List<byte[]> { new byte[] { 0x37, 0x7A, 0xBC, 0xAF } } }
        };

        /// <summary>
        /// Validate function for files
        /// </summary>
        /// <param name="file"></param>
        /// <param name="errorMessage"></param>
        /// <param name="maxFilSize">Maximum size of file in MB</param>
        /// <returns></returns>
        public static bool ValidateFile(IFormFile file, out string errorMessage, long maxFilSize = 10)
        {
            errorMessage = "";

            if (file == null || file.Length == 0)
            {
                errorMessage = "No file uploaded.";
                return false;
            }

            // Check extension
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !_allowedExtensions.Contains(ext))
            {
                errorMessage = $"File type '{ext}' is not supported.";
                return false;
            }

            // Check MIME
            if (!_allowedMimeTypes.Contains(file.ContentType))
            {
                errorMessage = $"File content type '{file.ContentType}' is not allowed.";
                return false;
            }

            // Check file signature (if known)
            if (_fileSignatures.TryGetValue(ext.ToLowerInvariant(), out var signatures))
            {
                using var reader = new BinaryReader(file.OpenReadStream());
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                bool validSignature = signatures.Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
                if (!validSignature)
                {
                    errorMessage = $"File signature mismatch for '{ext}'.";
                    return false;
                }
            }

            // Optional: Check file size (example 20 MB max)
            long maxFileSizeBytes = maxFilSize * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
            {
                errorMessage = $"File too large. Maximum allowed size is {maxFilSize} MB.";
                return false;
            }

            return true;
        }
    }
}
