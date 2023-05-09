# Releases

## 5/9/2923 0.3
- I made a whole bunch of stuff internal.  It is hoped that this will prevent breaking changes later on because 
no one will depend on the internal bits.  There may be some breaking changes, but I am taking them now when I hope
they will not hurt too much.  If I broke your use case, let me know.
- Every externally visible class and member has an xml doc comment.

## 12/17/2022 0.2
- Pdf 2.0 Compliance improvements throughout the parser.
- 256 bit AES encryption and decryption.
- Honor ColorTransform on a Dct encoded images.
- Update text rendering to use algorithm from 2.0 spec which covers a number of corner cases, including negative font sizes.
- Caching some intermediate values in shaders results in signifcant performance improvement.
- Detect some cases where invalid PDFs result in stack overflows.
- General improvements in code quality.

## 9/24/2022 0.1
- Fix a Wpf Rendering deadlock bug.
- Fix Version numbers to reflect semantic versioning.

## 9/17/2022 0.0.1
Initial Release