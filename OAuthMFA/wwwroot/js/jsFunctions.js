
window.getScreenDimensions = function () {
  return {
    width: window.screen.width,
    height: window.screen.height
  };
};

window.downloadFile = (filename, contentType, data) => {
  const blob = new Blob([data], { type: contentType });
  const url = URL.createObjectURL(blob);

  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = filename;
  anchorElement.click();

  URL.revokeObjectURL(url);
};

window.printHtmlContent = (htmlContent) => {
  const printWindow = window.open('', '_blank');
  printWindow.document.open();
  printWindow.document.write(`
            <html>
                <head>
                    <title>Print</title>
                    <style>
                        body { font-family: Arial, sans-serif; }
                        img { max-width: 100%; height: auto; }
                    </style>
                </head>
                <body onload="window.print(); window.close();">
                    ${htmlContent}
                </body>
            </html>
        `);
  printWindow.document.close();
};
